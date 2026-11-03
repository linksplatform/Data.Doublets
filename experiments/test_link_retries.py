"""Regressions for the upstream template's busy retry and false archive pass."""
import contextlib
import io
import os
from pathlib import Path
import sys
import tempfile
import unittest
from unittest.mock import patch
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
import recheck_broken_links as recheck
import check_web_archive as archive


class LinkRetryTests(unittest.TestCase):
    def test_connection_reset_gets_a_delayed_retry(self):
        with patch.object(recheck.time, 'sleep') as sleep:
            answers = iter([OSError('connection reset'), 200])
            def fetch(url, agent):
                result = next(answers)
                if isinstance(result, Exception):
                    raise result
                return result
            result = recheck.recheck_unanswered(['https://example.com'], accept='200', user_agent='lychee', budget_seconds=10, initial_wait_ms=1, fetch=fetch)
            self.assertEqual(result.recovered, ['https://example.com'])
            sleep.assert_called_once_with(0.001)

    def test_permanent_404_is_not_retried(self):
        with patch.object(recheck.time, 'sleep') as sleep:
            result = recheck.recheck_unanswered(['https://example.com'], accept='200', user_agent='lychee', fetch=lambda *_: 404)
            self.assertEqual(len(result.still_broken), 1)
            sleep.assert_not_called()

    def test_missing_report_never_claims_archive_success(self):
        with tempfile.TemporaryDirectory() as directory, patch.dict(os.environ, {'LYCHEE_OUTPUT': directory + '/missing.md', 'GITHUB_OUTPUT': directory + '/output'}), contextlib.redirect_stdout(io.StringIO()):
            self.assertEqual(archive.main(), 1)
            self.assertIn('all_archived=false', Path(directory + '/output').read_text())


if __name__ == '__main__':
    unittest.main()
