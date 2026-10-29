"""The public FFI header must compile as both C11 and C++20."""
from pathlib import Path
import shutil
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[1]


class FfiHeaderTests(unittest.TestCase):
    def test_standalone_c_and_cpp(self):
        for compiler, language, standard in [('cc', 'c', 'c11'), ('c++', 'c++', 'c++20')]:
            if not shutil.which(compiler):
                self.skipTest(f'{compiler} is not installed')
            with self.subTest(language=language):
                result = subprocess.run([compiler, '-x', language, '-std=' + standard, '-Werror', '-Wall', '-Wextra', '-I', str(ROOT), '-fsyntax-only', '-'], input='#include "c/ffi.h"\nint main(void) { return 0; }\n', text=True, capture_output=True)
                self.assertEqual(result.returncode, 0, result.stderr)


if __name__ == '__main__':
    unittest.main()
