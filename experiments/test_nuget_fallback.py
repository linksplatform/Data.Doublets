"""Issue 538: policy-incompatible verification and controlled credential retries."""
import contextlib
import io
import os
from pathlib import Path
import sys
import tempfile
import unittest
import urllib.error
from unittest.mock import Mock, patch

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
import release

PACKAGE = {'id': 'Example', 'version': '1.0.0', 'notes': ''}
OIDC = {'NUGET_USER': 'profile-name', 'NUGET_TOKEN': 'temporary-private',  # nosec B105 - Mocked credential; no network or real secret.
        'NUGET_LOGIN_OUTCOME': 'success'}
AUTHENTICATED = (200, b'{"Key":"derived-private","Expires":"2099-01-01T00:00:00Z"}')


class PreflightCompatibilityTests(unittest.TestCase):
    def test_live_400_after_exchange_cannot_be_a_false_scope_gate(self):
        def registry(url, **kwargs):
            if '/verifykey/' in url:
                return 400, b'The API key must have package:verify scope.'
            return 200, b'{"versions":["1.0.0"]}'
        for languages, credentials_only in ((['csharp', 'cpp'], True), (['csharp', 'cpp'], False), (['csharp'], False), (['cpp'], False)):
            with self.subTest(languages=languages, credentials_only=credentials_only):
                with patch.dict(os.environ, OIDC, clear=True), patch.object(release, 'request', side_effect=registry) as requests, patch.object(release, 'verify_github', return_value=None), contextlib.redirect_stdout(io.StringIO()) as output:
                    release.preflight('release', languages, credentials_only)
                self.assertIn('scope', output.getvalue())
                self.assertIn('unverified', output.getvalue())
                self.assertFalse(any('/verifykey/' in call.args[0] for call in requests.call_args_list))

    def test_http_error_body_is_retained_but_bounded_and_redacted(self):
        body = b'policy rejected temporary-private\n::error::injected ' + b'x' * 10000
        error = urllib.error.HTTPError('https://www.nuget.org/', 400, 'Bad Request', {}, io.BytesIO(body))
        opener = Mock()
        opener.open.side_effect = error
        with patch.dict(os.environ, OIDC, clear=True), patch.object(release.urllib.request, 'build_opener', return_value=opener):
            status, diagnostic = release.request('https://www.nuget.org/')
            rendered = release.diagnostic(diagnostic)
        self.assertEqual(status, 400)
        self.assertIn('policy rejected', rendered)
        self.assertNotIn('temporary-private', rendered)
        self.assertNotIn('\n', rendered)
        self.assertLessEqual(len(rendered), 1024)
        self.assertLessEqual(len(diagnostic), 8192)

    def test_transport_exception_never_exposes_secret_headers(self):
        opener = Mock()
        opener.open.side_effect = urllib.error.URLError('headers: temporary-private')
        with patch.object(release.urllib.request, 'build_opener', return_value=opener):
            self.assertEqual(release.request('https://www.nuget.org/'), (0, b''))


class AuthenticationDecisionTests(unittest.TestCase):
    def select(self, environment):
        with patch.dict(os.environ, environment, clear=True), contextlib.redirect_stdout(io.StringIO()) as output:
            selected = release.select_nuget_auth(['csharp', 'cpp'])
        return selected, output.getvalue()

    def test_oidc_preferred_over_every_existing_fallback(self):
        for fallback in ('', 'valid-provided', 'expired-provided'):
            with self.subTest(fallback=fallback), patch.object(release, 'request') as request:
                selected, output = self.select({**OIDC, 'NUGET_FALLBACK_TOKEN': fallback})
                self.assertEqual(selected, ('trusted-publishing', OIDC['NUGET_TOKEN']))
                request.assert_not_called()
                self.assertIn('auth=trusted-publishing', output)
                if fallback:
                    self.assertNotIn(fallback, output)

    def test_unavailable_or_failed_oidc_considers_provided_token(self):
        for user, outcome in (('', 'skipped'), ('email@example.org', 'skipped'), ('profile-name', 'failure'), ('profile-name', 'success')):
            with self.subTest(user=user, outcome=outcome), patch.object(release, 'request', return_value=AUTHENTICATED) as request:
                selected, output = self.select({'NUGET_USER': user, 'NUGET_LOGIN_OUTCOME': outcome, 'NUGET_FALLBACK_TOKEN': 'provided-private'})  # nosec B105 - Mocked credential; no network or real secret.
                self.assertEqual(selected, ('token-fallback', 'provided-private'))
                self.assertIn('failed-step=', output)
                self.assertIn('auth=token-fallback', output)
                self.assertNotIn('provided-private', output)
                self.assertIn('/create-verification-key/', request.call_args.args[0])
                self.assertNotIn('scope verified', output)

    def test_failed_oidc_without_fallback_reports_both_outcomes(self):
        with self.assertRaisesRegex(RuntimeError, 'trusted.*failed.*fallback.*missing'):
            self.select({'NUGET_USER': 'profile-name', 'NUGET_LOGIN_OUTCOME': 'failure'})

    def test_expired_revoked_and_wrong_action_tokens_fail_authentication(self):
        for status in (401, 403):
            with self.subTest(status=status), patch.object(release, 'request', return_value=(status, b'provided-private rejected')):
                with self.assertRaisesRegex(RuntimeError, f'trusted.*failed.*fallback.*HTTP {status}') as error:
                    self.select({'NUGET_USER': 'profile-name', 'NUGET_LOGIN_OUTCOME': 'failure', 'NUGET_FALLBACK_TOKEN': 'provided-private'})  # nosec B105 - Mocked credential; no network or real secret.
                self.assertNotIn('provided-private', str(error.exception))

    def test_fallback_authentication_is_not_derived_package_scope_proof(self):
        with patch.dict(os.environ, {'NUGET_FALLBACK_TOKEN': 'provided-private'}, clear=True), patch.object(release, 'request', return_value=AUTHENTICATED) as request, contextlib.redirect_stdout(io.StringIO()) as output:  # nosec B105 - Mocked credential; no network or real secret.
            self.assertEqual(release.select_nuget_auth(['cpp'])[0], 'token-fallback')
        self.assertEqual(request.call_count, 1)
        self.assertIn('scope', output.getvalue())
        self.assertIn('unverified', output.getvalue())
        self.assertNotIn('derived-private', output.getvalue())

    def test_malformed_or_unsupported_authentication_is_not_owner_failure(self):
        for response in ((200, b'{}'), (400, b'unsupported'), (503, b'unavailable'), (0, b'')):
            with self.subTest(response=response), patch.object(release, 'request', return_value=response):
                with self.assertRaises(RuntimeError) as error:
                    self.select({'NUGET_FALLBACK_TOKEN': 'provided-private'})  # nosec B105 - Mocked credential; no network or real secret.
                self.assertNotIn('owner/package scope', str(error.exception))


class PushRetryTests(unittest.TestCase):
    def push(self, responses, public_status=404, environment=None):
        environment = {**OIDC, 'NUGET_FALLBACK_TOKEN': 'provided-private', **(environment or {})}  # nosec B105 - Mocked credential; no network or real secret.
        with tempfile.TemporaryDirectory() as directory:
            artifact = Path(directory) / 'Example.1.0.0.nupkg'
            artifact.write_bytes(b'PKfixture')
            with patch.dict(os.environ, environment, clear=True), patch.object(release, 'request', return_value=(public_status, b'PKfixture')) as request, patch.object(release, 'authenticate_fallback'), patch.object(release, 'run_nuget_push', side_effect=responses) as push, contextlib.redirect_stdout(io.StringIO()) as output:
                release.push_nuget(artifact, PACKAGE, 'trusted-publishing', OIDC['NUGET_TOKEN'])
            return artifact, push.call_args_list, request.call_args_list, output.getvalue()

    def test_auth_rejection_checks_exact_public_version_then_reuses_artifact(self):
        for status in (401, 403):
            with self.subTest(status=status):
                artifact, calls, checks, output = self.push([(1, f'error: Response status code does not indicate success: {status} (Forbidden).'), (0, 'success')])
                self.assertEqual(len(calls), 2)
                self.assertEqual(calls[0].args[0], artifact)
                self.assertEqual(calls[1].args[0], artifact)
                self.assertEqual(calls[0].args[-1], OIDC['NUGET_TOKEN'])
                self.assertEqual(calls[1].args[-1], 'provided-private')
                self.assertIn('/example/1.0.0/example.1.0.0.nupkg', checks[0].args[0])
                self.assertFalse(checks[0].kwargs)
                self.assertIn('auth=token-fallback', output)

    def test_already_public_ambiguous_push_is_reported_for_duplicate_safe_repair(self):
        _, calls, _, output = self.push([(1, 'HTTP 403 Forbidden'), (0, 'duplicate skipped; symbols repaired')], public_status=200)
        self.assertEqual(len(calls), 2)
        self.assertIn('already public', output)

    def test_success_never_tests_expired_fallback(self):
        with patch.object(release, 'authenticate_fallback') as fallback:
            _, calls, checks, output = self.push([(0, 'success')], environment={'NUGET_FALLBACK_TOKEN': 'expired-provided'})  # nosec B105 - Mocked credential; no network or real secret.
            fallback.assert_not_called()
        self.assertEqual(len(calls), 1)
        self.assertEqual(checks, [])
        self.assertIn('auth=trusted-publishing', output)

    def test_validation_transport_and_unrelated_errors_cannot_switch_credentials(self):
        for message in ('HTTP 400 Bad Request', 'HTTP 500 Internal Server Error', 'HTTP 429 Too Many Requests', 'connection timed out', 'invalid package', 'warning: package has 403 types'):
            with self.subTest(message=message), patch.object(release, 'authenticate_fallback') as fallback:
                with self.assertRaises(RuntimeError):
                    self.push([(1, message)])
                fallback.assert_not_called()

    def test_registry_rejection_with_missing_or_rejected_fallback_fails(self):
        for fallback in ('', 'provided-private'):
            with self.subTest(fallback=bool(fallback)):
                with self.assertRaisesRegex(RuntimeError, 'trusted.*HTTP 403.*fallback'):
                    self.push([(1, 'HTTP 403 Forbidden'), (1, 'HTTP 401 Unauthorized')], environment={'NUGET_FALLBACK_TOKEN': fallback})

    def test_indeterminate_public_lookup_prevents_auth_retry(self):
        with self.assertRaisesRegex(RuntimeError, 'HTTP 503'):
            self.push([(1, 'HTTP 403 Forbidden')], public_status=503)

    def test_retry_is_bounded_and_sanitizes_both_outcomes(self):
        with self.assertRaises(RuntimeError) as error:
            self.push([(1, 'HTTP 403 temporary-private'), (1, 'HTTP 403 provided-private')])
        self.assertIn('trusted', str(error.exception))
        self.assertIn('fallback', str(error.exception))
        self.assertNotIn('temporary-private', str(error.exception))
        self.assertNotIn('provided-private', str(error.exception))


class ReleaseFailureTests(unittest.TestCase):
    def test_rejected_publication_cannot_promote_a_new_release_from_draft(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            (output / 'Example.1.0.0.nupkg').write_bytes(b'PKfixture')
            with patch.dict(os.environ, {**OIDC, 'GH_TOKEN': 'github-private'}, clear=True), patch.object(release, 'info', return_value=PACKAGE), patch.object(release, 'request', return_value=(404, b'')), patch.object(release, 'push_nuget', side_effect=RuntimeError('both authentications rejected')), patch.object(release, 'run_nuget_push', return_value=(0, 'mirror success')), patch.object(release.subprocess, 'check_output', return_value='fixture-sha'), patch.object(release.subprocess, 'run') as commands, contextlib.redirect_stdout(io.StringIO()):  # nosec B105 - Mocked credential; no network or real secret.
                with self.assertRaisesRegex(RuntimeError, 'both authentications rejected'):
                    release.publish('csharp', output)
            self.assertTrue(any(call.args[0][:3] == ['gh', 'release', 'create'] and '--draft' in call.args[0] for call in commands.call_args_list))
            self.assertFalse(any('--draft=false' in call.args[0] for call in commands.call_args_list))

    def test_push_client_captures_output_and_preserves_duplicate_and_symbol_behavior(self):
        def run(command, stdout, stderr, **kwargs):
            self.assertIs(stdout, stderr)
            stdout.write(b'HTTP 403 secret-private\n')
            return Mock(returncode=1)
        with patch.object(release.subprocess, 'run', side_effect=run) as command, contextlib.redirect_stdout(io.StringIO()) as output:
            code, body = release.run_nuget_push(Path('fixture.nupkg'), 'https://api.nuget.org/v3/index.json', 'secret-private')
        self.assertEqual(code, 1)
        self.assertIn('HTTP 403', body)
        self.assertEqual(output.getvalue(), '')
        self.assertIn('--skip-duplicate', command.call_args.args[0])
        self.assertNotIn('--no-symbols', command.call_args.args[0])


class RepositoryAuthPolicyTests(unittest.TestCase):
    def test_policy_rejects_unconditional_token_and_ignored_publication(self):
        import copy
        import yaml
        import check_repository
        workflow = yaml.safe_load((ROOT / '.github/workflows/ci.yml').read_text())
        self.assertEqual(check_repository.trusted_publishing_errors(workflow), [])
        for change in ('direct-token', 'ignore-publish', 'ignore-job', 'cross-job-key'):
            invalid = copy.deepcopy(workflow)
            job = invalid['jobs']['publish']
            caller = next(step for step in job['steps'] if 'publish --language csharp' in step.get('run', ''))
            if change == 'direct-token':
                caller['env']['NUGET_TOKEN'] = '${{ secrets.NUGET_TOKEN }}'  # nosec B105 - Mocked credential; no network or real secret.
            elif change == 'ignore-publish':
                caller['continue-on-error'] = True
            elif change == 'ignore-job':
                job['continue-on-error'] = True
            else:
                job['outputs'] = {'key': '${{ steps.nuget-login.outputs.NUGET_API_KEY }}'}
            with self.subTest(change=change):
                self.assertTrue(check_repository.trusted_publishing_errors(invalid))


if __name__ == '__main__':
    unittest.main()
