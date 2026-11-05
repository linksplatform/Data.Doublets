"""Regressions for upstream recipe diagnostics that escaped the original CI."""
from pathlib import Path
import os
import shutil
import subprocess
import sys
import tempfile
from types import SimpleNamespace
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
from patch_conan_recipes import modern_metadata, dependency_source_compatibility, package_id_metadata


class CompatibilityTests(unittest.TestCase):
    @unittest.skipUnless(shutil.which('conan'), 'Conan is required')
    def test_date_version_exports_strictly_from_an_empty_cache(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            recipe = root / 'conanfile.py'
            source = 'from conan import ConanFile\nclass CpuinfoConan(ConanFile):\n    name = "cpuinfo"\n'
            recipe.write_text(source)
            command = ['conan', 'export', str(recipe), '--version=cci.20220228',
                       '-cc=core:warnings_as_errors=["*"]']
            environment = {**os.environ, 'CONAN_HOME': str(root / 'cache')}
            before = subprocess.run(command, env=environment, capture_output=True, text=True)
            self.assertNotEqual(before.returncode, 0)
            self.assertIn('alphanumeric major', before.stderr)
            recipe.write_text(package_id_metadata(source, 'cpuinfo'))
            after = subprocess.run(command, env=environment, capture_output=True, text=True)
            self.assertEqual(after.returncode, 0, after.stderr)

    @unittest.skipUnless(shutil.which('cc'), 'A POSIX C compiler is required')
    def test_clog_short_writes_and_interruption_preserve_the_whole_message(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / 'deps/clog/src/clog.c'
            source.parent.mkdir(parents=True)
            fixture = ROOT / 'dev/log/issues/524/pulls/526/research'
            shutil.copyfile(fixture / 'cpuinfo-clog-current.c', source)
            shutil.copyfile(fixture / 'cpuinfo-clog-current.h', root / 'clog.h')
            command = ['cc', '-std=c11', '-O2', '-D_FORTIFY_SOURCE=2',
                       f'-DCLOG_SOURCE_PATH="{source}"', '-I' + str(root),
                       str(ROOT / 'experiments/clog_partial_write.c'), '-o', str(root / 'probe')]
            subprocess.run(command, check=True, capture_output=True)
            self.assertEqual(subprocess.run([str(root / 'probe')]).returncode, 1)
            dependency_source_compatibility(SimpleNamespace(source_folder=directory, name='cpuinfo'))
            subprocess.run([*command, '-Werror=unused-result'], check=True, capture_output=True)
            subprocess.run([str(root / 'probe')], check=True)

    def test_legacy_metadata_without_final_newline_is_removed(self):
        self.assertEqual(modern_metadata('        self.cpp_info.names["cmake_find_package"] = "Memory"'), '')

    def test_modern_targets_are_preserved(self):
        modern = '        self.cpp_info.set_property("cmake_target_name", "Memory")\n'
        self.assertEqual(modern_metadata(modern + '        self.cpp_info.names["legacy"] = "Memory"\n'), modern)

    def test_unexpected_clog_source_fails_closed(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'deps/clog/src/clog.c'
            path.parent.mkdir(parents=True)
            path.write_text('#include <clog.h>\n')
            with self.assertRaisesRegex(ValueError, 'five'):
                dependency_source_compatibility(SimpleNamespace(source_folder=directory, name='cpuinfo'))

    def test_cmake_minimum_repair_preserves_newer_requirements(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            old = root / 'CMakeLists.txt'
            old.write_text('cmake_minimum_required (VERSION 2.8.12)\n')
            nested = root / 'new/CMakeLists.txt'
            nested.parent.mkdir()
            nested.write_text('cmake_minimum_required(VERSION 3.20)\n')
            dependency_source_compatibility(SimpleNamespace(source_folder=directory, name='ms-gsl'))
            self.assertIn('VERSION 3.13', old.read_text())
            self.assertIn('VERSION 3.20', nested.read_text())

    def test_cmake_version_range_preserves_its_upper_policy_version(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'CMakeLists.txt'
            path.write_text('cmake_minimum_required(VERSION 3.1...3.16)\n')
            dependency_source_compatibility(SimpleNamespace(source_folder=directory, name='ms-gsl'))
            self.assertIn('VERSION 3.13...3.16', path.read_text())


if __name__ == '__main__':
    unittest.main()
