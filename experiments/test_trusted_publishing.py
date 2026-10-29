"""Issue 535: trusted publishing must authenticate and preserve package scope."""
import contextlib
import io
import json
import os
from pathlib import Path
import sys
import unittest
from unittest.mock import patch

import yaml

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
import release
import check_repository


class TrustedPreflightTests(unittest.TestCase):
    def preflight(self, environment, mode='release'):
        output = io.StringIO()
        with patch.dict(os.environ, environment, clear=True), contextlib.redirect_stdout(output):
            release.preflight(mode, ['csharp', 'cpp'], credentials_only=True)
        return output.getvalue()

    def test_missing_username_never_uses_a_stored_key(self):
        with patch.object(release, 'request') as request:
            with self.assertRaisesRegex(RuntimeError, 'preflight'):
                self.preflight({'NUGET_TOKEN': 'legacy-secret'})  # nosec B105 - Mocked credential, never sent to NuGet.
            request.assert_not_called()

    def test_username_alone_does_not_prove_oidc_exchange(self):
        with self.assertRaisesRegex(RuntimeError, 'preflight'):
            self.preflight({'NUGET_USER': 'profile-name'})

    def test_missing_or_email_profile_has_actionable_setup_guidance(self):
        for user in ('', 'person@example.org', ' profile ', 'bad\nname'):
            with self.subTest(user=user), patch.dict(os.environ, {'NUGET_USER': user}, clear=True):
                with self.assertRaisesRegex(RuntimeError, 'NUGET_USER.*profile'):
                    release.check_auth_config()

    def test_report_without_credentials_makes_no_network_requests(self):
        with patch.object(release, 'request') as request:
            output = self.preflight({}, mode='report')
            request.assert_not_called()
        self.assertIn('trusted publishing', output)
        self.assertIn('NUGET_USER', output)
        self.assertNotIn('Rotate', output)

    def test_report_with_credentials_does_not_claim_verified_authentication(self):
        with patch.object(release, 'request') as request:
            output = self.preflight({'NUGET_USER': 'profile-name', 'NUGET_TOKEN': 'private'}, mode='report')  # nosec B105 - Mocked credential.
            request.assert_not_called()
        self.assertIn('unverified in report mode', output)
        self.assertNotIn('exchange succeeded', output)

    def test_exchange_failure_command_names_policy_repairs_without_a_key(self):
        import subprocess  # nosec B404 - Exercise the repository's CLI error contract.
        result = subprocess.run([sys.executable, str(ROOT / 'scripts/release.py'), 'auth-error'], capture_output=True, text=True, check=False)  # nosec B603 - Fixed arguments to the local repository script.
        self.assertNotEqual(result.returncode, 0)
        for guidance in ('username', 'workflow/environment', 'inactive policy', 'ownership/scope', 'ci.yml'):
            self.assertIn(guidance, result.stderr)

    def test_oidc_scope_check_uses_original_key_not_legacy_symbol_key(self):
        responses = [(200, json.dumps({'versions': ['1.0.0']}).encode()), (200, b'')]
        with patch.object(release, 'request', side_effect=responses) as request:
            self.assertIsNone(release.verify_nuget('temporary-private', {'id': 'Example'}))
        self.assertIn('/verifykey/Example/1.0.0', request.call_args.args[0])
        self.assertEqual(request.call_args.kwargs['headers']['X-NuGet-ApiKey'], 'temporary-private')
        self.assertTrue(all('create-verification-key' not in call.args[0] for call in request.call_args_list))

    def test_rejected_scope_is_not_ignored_as_oidc_incompatibility(self):
        for status in (401, 403, 500):
            with self.subTest(status=status), patch.object(release, 'request', side_effect=[(200, b'{"versions":["1.0.0"]}'), (status, b'')]):
                error = release.verify_nuget('temporary-private', {'id': 'Example'})
                self.assertIn(str(status), error)
                self.assertIn('owner', error)
                self.assertNotIn('temporary-private', error)

    def test_first_publication_reports_scope_unverified(self):
        with patch.object(release, 'request', return_value=(404, b'')), patch('sys.stdout', new_callable=io.StringIO) as output:
            self.assertIsNone(release.verify_nuget('private', {'id': 'Example'}))
        self.assertIn('unverified', output.getvalue())
        self.assertIn('push', output.getvalue())

    def test_existing_package_checks_both_scopes_after_exchange(self):
        with patch.object(release, 'verify_nuget', return_value=None) as verify:
            output = self.preflight({'NUGET_USER': 'profile-name', 'NUGET_TOKEN': 'temporary-private'})  # nosec B105 - Mocked credential.
        self.assertEqual({call.args[1]['id'] for call in verify.call_args_list}, {'Platform.Data.Doublets', 'Platform.Data.Doublets.TemplateLibrary'})
        self.assertIn('exchange succeeded', output)
        self.assertNotIn('temporary-private', output)

    def test_malformed_index_fails_closed(self):
        for body in (b'null', b'[]', b'{"versions":[]}', b'{"versions":[null]}', b'{"versions":["../../host"]}'):
            with self.subTest(body=body), patch.object(release, 'request', return_value=(200, body)):
                self.assertIn('malformed', release.verify_nuget('private', {'id': 'Example'}))


class TrustedWorkflowTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.source = (ROOT / '.github/workflows/ci.yml').read_text()
        cls.jobs = yaml.safe_load(cls.source)['jobs']

    def test_oidc_is_job_scoped_and_trusted_main_only(self):
        for name, job in self.jobs.items():
            if name in ('credentials', 'publish'):
                self.assertEqual(job['permissions']['id-token'], 'write')
                self.assertIn("github.ref == 'refs/heads/main'", job['if'])
                self.assertIn("github.event_name != 'pull_request'", job['if'])
            else:
                self.assertNotIn('id-token', job.get('permissions', {}))

    def test_login_is_pinned_and_precedes_prebuild_gate(self):
        steps = self.jobs['credentials']['steps']
        login = next(i for i, step in enumerate(steps) if step.get('uses', '').startswith('NuGet/login@'))
        self.assertEqual(steps[login]['uses'], 'NuGet/login@8d196754b4036150537f80ac539e15c2f1028841')
        self.assertEqual(steps[login]['with']['user'], '${{ vars.NUGET_USER }}')
        gate = next(i for i, step in enumerate(steps) if '--credentials-only' in step.get('run', ''))
        self.assertLess(login, gate)
        self.assertIn('outputs.NUGET_API_KEY', steps[gate]['env']['NUGET_TOKEN'])
        self.assertNotIn('outputs', self.jobs['credentials'])

    def test_authentication_before_metadata_and_fresh_key_for_each_push(self):
        steps = self.jobs['publish']['steps']
        metadata = next(i for i, step in enumerate(steps) if 'scripts/versions.py commit' in step.get('run', ''))
        login_positions = [i for i, step in enumerate(steps) if step.get('uses', '').startswith('NuGet/login@')]
        self.assertLess(login_positions[0], metadata)
        for language in ('csharp', 'cpp'):
            push = next(i for i, step in enumerate(steps) if f'publish --language {language}' in step.get('run', ''))
            login = max(i for i in login_positions if i < push)
            self.assertIn(f"steps.{steps[login]['id']}.outputs.NUGET_API_KEY", steps[push]['env']['NUGET_TOKEN'])
            self.assertFalse(any('publish --language' in step.get('run', '') for step in steps[login + 1:push]))
        # NuGet keys last one hour; the entire writer job is bounded below that.
        self.assertLess(self.jobs['publish']['timeout-minutes'], 60)
        self.assertEqual(self.jobs['publish']['permissions']['contents'], 'write')
        self.assertEqual(self.jobs['publish']['permissions']['packages'], 'write')

    def test_rejected_exchange_has_policy_guidance_in_each_job(self):
        for name in ('credentials', 'publish'):
            steps = self.jobs[name]['steps']
            for login in [step for step in steps if step.get('uses', '').startswith('NuGet/login@')]:
                self.assertTrue(any(f"steps.{login['id']}.outcome == 'failure'" in step.get('if', '') and 'auth-error' in step.get('run', '') for step in steps))

    def test_no_legacy_fallback_and_no_ephemeral_key_in_artifacts(self):
        self.assertNotIn('secrets.NUGET_TOKEN', self.source)
        self.assertNotIn('secrets.NUGET_API_KEY', self.source)
        for job in self.jobs.values():
            self.assertNotIn('NUGET_API_KEY', json.dumps(job.get('outputs', {})))
        self.assertFalse(any('upload-artifact@' in step.get('uses', '') for step in self.jobs['credentials']['steps']))

    def test_repository_checker_rejects_legacy_authentication_regression(self):
        invalid = self.source.replace('NUGET_TOKEN: ${{ steps.nuget-login.outputs.NUGET_API_KEY }}', 'NUGET_TOKEN: ${{ secrets.NUGET_TOKEN }}')
        self.assertNotEqual(invalid, self.source)
        self.assertTrue(check_repository.trusted_publishing_errors(yaml.safe_load(invalid)))
        self.assertEqual(check_repository.trusted_publishing_errors(yaml.safe_load(self.source)), [])


if __name__ == '__main__':
    unittest.main()
