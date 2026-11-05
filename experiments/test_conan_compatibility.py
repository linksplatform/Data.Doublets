"""Regressions for upstream recipe diagnostics that escaped the original CI."""
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
from patch_conan_recipes import modern_metadata, package_id_metadata


class CompatibilityTests(unittest.TestCase):
    def test_legacy_metadata_without_final_newline_is_removed(self):
        self.assertEqual(modern_metadata('        self.cpp_info.names["cmake_find_package"] = "Memory"'), '')

    def test_modern_targets_are_preserved(self):
        modern = '        self.cpp_info.set_property("cmake_target_name", "Memory")\n'
        self.assertEqual(modern_metadata(modern + '        self.cpp_info.names["legacy"] = "Memory"\n'), modern)

    def test_component_metadata_is_removed_without_removing_modern_target(self):
        source = ('        self.cpp_info.components["_ms-gsl"].names["cmake_find_package"] = "GSL"\n'
                  '        self.cpp_info.components["_ms-gsl"].set_property("cmake_target_name", "Microsoft.GSL::GSL")\n')
        self.assertEqual(modern_metadata(source), source.split('\n', 1)[1])
    def test_date_version_uses_complete_identity_in_all_relationships(self):
        source = 'class CpuinfoConan(ConanFile):\n    pass\n'
        patched = package_id_metadata(source, 'cpuinfo')
        self.assertIn('package_id_non_embed_mode = "full_mode"', patched)
        self.assertIn('package_id_unknown_mode = "full_mode"', patched)
        self.assertEqual(package_id_metadata(source, 'ms-gsl'), source)


if __name__ == '__main__':
    unittest.main()
