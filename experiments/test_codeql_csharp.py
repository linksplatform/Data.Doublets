"""CodeQL must compile executable projects separately and cover every project."""
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

import yaml

ROOT = Path(__file__).resolve().parents[1]


class CodeqlCsharpTests(unittest.TestCase):
    def test_csharp_uses_manual_extraction_with_the_pinned_sdk(self):
        workflow = yaml.safe_load((ROOT / '.github/workflows/security.yml').read_text())
        job = workflow['jobs']['codeql']
        configurations = job['strategy']['matrix']['include']
        csharp = next(item for item in configurations if item['language'] == 'csharp')
        self.assertEqual(csharp['build-mode'], 'manual')
        steps = job['steps']
        setup = next(step for step in steps if step.get('uses', '').startswith('actions/setup-dotnet@'))
        self.assertEqual(setup['if'], "matrix.language == 'csharp'")
        self.assertEqual(setup['with']['global-json-file'], 'global.json')
        build = next(step for step in steps if step.get('run') == 'bash scripts/build-codeql-csharp.sh')
        self.assertEqual(build['if'], "matrix.language == 'csharp'")
        init = next(step for step in steps if step.get('uses', '').startswith('github/codeql-action/init@'))
        analyze = next(step for step in steps if step.get('uses', '').startswith('github/codeql-action/analyze@'))
        self.assertLess(steps.index(init), steps.index(build))
        self.assertLess(steps.index(setup), steps.index(build))
        self.assertLess(steps.index(build), steps.index(analyze))

    def test_every_tracked_project_is_rebuilt_separately_for_the_tracer(self):
        projects = subprocess.check_output(['git', 'ls-files', '-z', '*.csproj'], cwd=ROOT).decode().strip('\0').split('\0')
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            log = folder / 'builds.jsonl'
            dotnet = folder / 'dotnet'
            dotnet.write_text('#!/usr/bin/env python3\nimport json, os, sys\nwith open(os.environ["CODEQL_TEST_LOG"], "a") as log:\n    log.write(json.dumps(sys.argv[1:]) + "\\n")\n')
            dotnet.chmod(0o755)
            environment = dict(os.environ, PATH=f'{folder}{os.pathsep}{os.environ["PATH"]}', CODEQL_TEST_LOG=str(log))
            subprocess.run(['bash', str(ROOT / 'scripts/build-codeql-csharp.sh')], cwd=folder, env=environment, check=True)
            builds = [json.loads(line) for line in log.read_text().splitlines()]
        self.assertEqual(sorted(build[1] for build in builds), sorted(projects))
        for build in builds:
            self.assertEqual(build[0], 'build')
            for option in ('--no-incremental', '-m:1', '-warnaserror', '-p:UseSharedCompilation=false'):
                self.assertIn(option, build)


if __name__ == '__main__':
    unittest.main()
