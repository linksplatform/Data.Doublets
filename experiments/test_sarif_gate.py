"""A successful analysis/upload must not hide CodeQL findings or missing output."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('check_sarif', Path(__file__).resolve().parents[1] / 'scripts/check_sarif.py')
gate = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gate)


class SarifGateTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory()
        self.addCleanup(self.directory.cleanup)
        self.run = {'tool': {'driver': {'name': 'CodeQL'}}, 'results': []}

    def write(self):
        Path(self.directory.name, 'report.sarif').write_text(json.dumps({'version': '2.1.0', 'runs': [self.run]}))

    def test_clean_complete_report_passes(self):
        self.write()
        gate.check(self.directory.name)

    def test_notes_warnings_and_errors_fail_even_after_successful_analysis(self):
        for level in ('note', 'warning', 'error'):
            with self.subTest(level=level):
                self.run['results'] = [{'level': level, 'ruleId': 'test/finding', 'message': {'text': 'problem'}}]
                self.write()
                with self.assertRaisesRegex(ValueError, 'test/finding'):
                    gate.check(self.directory.name)

    def test_missing_report_fails(self):
        with self.assertRaisesRegex(ValueError, 'did not produce'):
            gate.check(self.directory.name)

    def test_incomplete_report_fails(self):
        self.run.pop('results')
        self.write()
        with self.assertRaisesRegex(ValueError, 'Incomplete'):
            gate.check(self.directory.name)

    def test_analysis_failure_and_diagnostics_fail(self):
        for invocation in ({'executionSuccessful': False}, {'toolExecutionNotifications': [{'level': 'warning'}]}):
            self.run['invocations'] = [invocation]
            self.write()
            with self.assertRaises(ValueError):
                gate.check(self.directory.name)
