"""Publication reruns preserve immutable public releases and repair missing assets."""

import json
import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
import release


class ReleasePreservationTests(unittest.TestCase):
    def publish(self, output, before, after, language='csharp'):
        with (
            patch.object(release, 'info', return_value={'id': 'Example', 'version': '1.0.0', 'notes': ''}),
            patch.object(release, 'request', side_effect=[(200, before), (200, after)]),
            patch.dict(os.environ, {'GH_TOKEN': 'fixture'}),  # nosec B105 - Mock credentials; all network calls are patched.
            patch.object(release, 'select_nuget_auth', return_value=('trusted-publishing', 'fixture')),
            patch.object(release, 'push_nuget') as push,
            patch.object(release, 'wait_for_package') as wait,
            patch.object(release, 'run_nuget_push', return_value=(0, '')) as mirror,
            patch.object(release.subprocess, 'run') as commands,
        ):
            release.publish(language, output)
            push.assert_called_once()
            wait.assert_called_once()
            mirror.assert_called_once()
            return [call.args[0] for call in commands.call_args_list]

    def test_existing_public_release_preserves_original_assets_on_same_version_rebuild(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            (output / 'Example.1.0.0.nupkg').write_bytes(b'PK-rebuilt')
            (output / 'Example.1.0.0.snupkg').write_bytes(b'PK-symbols-rebuilt')
            # A shared output directory must not attach a new native ZIP to C#.
            (output / 'platform.data.doublets_2.0.0.zip').write_bytes(b'PK-native')
            public = json.dumps({'draft': False, 'assets': [
                {'name': 'Example.1.0.0.nupkg', 'size': 2},
                {'name': 'Example.1.0.0.snupkg', 'size': 3},
                {'name': 'platform.data.doublets_0.9.0.zip', 'size': 4},
            ]}).encode()
            self.assertEqual(self.publish(output, public, public), [])

    def test_existing_public_release_uploads_only_missing_assets(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            (output / 'Example.1.0.0.nupkg').write_bytes(b'PK-rebuilt')
            symbols = output / 'Example.1.0.0.snupkg'
            symbols.write_bytes(b'PK-symbols')
            before = {'draft': False, 'assets': [{'name': 'Example.1.0.0.nupkg', 'size': 2}]}
            after = {'draft': False, 'assets': before['assets'] + [
                {'name': symbols.name, 'size': symbols.stat().st_size},
            ]}
            commands = self.publish(output, json.dumps(before).encode(), json.dumps(after).encode())
            self.assertEqual(len(commands), 1)
            self.assertEqual(commands[0][:3], ['gh', 'release', 'upload'])
            self.assertEqual(commands[0][-1], str(symbols))
            self.assertNotIn('--clobber', commands[0])

    def test_unpublished_draft_replaces_invalid_assets_and_checks_exact_native_zip(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            artifact = output / 'Example.1.0.0.nupkg'
            artifact.write_bytes(b'PK-fixed')
            archive = output / 'platform.data.doublets_1.0.0.zip'
            archive.write_bytes(b'PK-native')
            (output / 'platform.data.doublets_0.9.0.zip').write_bytes(b'PK-old')
            before = json.dumps({'draft': True, 'assets': [{'name': artifact.name, 'size': 2}]}).encode()
            after = json.dumps({'draft': False, 'assets': [
                {'name': path.name, 'size': path.stat().st_size} for path in (artifact, archive)
            ]}).encode()
            commands = self.publish(output, before, after, 'cpp')
            self.assertEqual(len(commands), 2)
            self.assertEqual(commands[0][-2:], [str(artifact), str(archive)])
            self.assertIn('--clobber', commands[0])
            self.assertIn('--draft=false', commands[1])

    def test_malformed_existing_release_fails_before_any_publication(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            (output / 'Example.1.0.0.nupkg').write_bytes(b'PK')
            for body in (b'not-json', b'{}', b'{"draft":false,"assets":null}'):
                with (
                    self.subTest(body=body),
                    patch.object(release, 'info', return_value={'id': 'Example', 'version': '1.0.0'}),
                    patch.object(release, 'request', return_value=(200, body)),
                    patch.dict(os.environ, {'GH_TOKEN': 'fixture'}),  # nosec B105 - Mock credentials; all network calls are patched.
                    patch.object(release, 'select_nuget_auth') as authenticate,
                    patch.object(release.subprocess, 'run') as commands,
                ):
                    with self.assertRaisesRegex(RuntimeError, 'malformed'):
                        release.publish('csharp', output)
                    authenticate.assert_not_called()
                    commands.assert_not_called()

    def test_anonymous_verification_cannot_lose_a_preserved_asset(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            artifact = output / 'Example.1.0.0.nupkg'
            artifact.write_bytes(b'PK')
            before = json.dumps({'draft': False, 'assets': [{'name': artifact.name, 'size': 2}]}).encode()
            after = json.dumps({'draft': False, 'assets': []}).encode()
            with self.assertRaisesRegex(RuntimeError, 'incomplete assets'):
                self.publish(output, before, after)

    def test_empty_public_asset_is_repaired(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            artifact = output / 'Example.1.0.0.nupkg'
            artifact.write_bytes(b'PK')
            before = json.dumps({'draft': False, 'assets': [{'name': artifact.name, 'size': 0}]}).encode()
            after = json.dumps({'draft': False, 'assets': [{'name': artifact.name, 'size': 2}]}).encode()
            commands = self.publish(output, before, after)
            self.assertEqual(len(commands), 1)
            self.assertIn('--clobber', commands[0])
            self.assertEqual(commands[0][-1], str(artifact))


if __name__ == '__main__':
    unittest.main()
