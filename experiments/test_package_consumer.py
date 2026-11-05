"""The isolated native consumer must not reuse CMake's build output paths."""
import json
from pathlib import Path
import shlex
import shutil
import sys
import tempfile
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
import smoke_packages


class NativeConsumerTests(unittest.TestCase):
    @unittest.skipUnless(shutil.which('c++'), 'A C++ compiler is required')
    def test_cmake_link_dependencies_do_not_escape_the_isolated_consumer(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            build = root / 'build'
            target = build / 'CMakeFiles/Platform.Data.Doublets.Tests.dir'
            target.mkdir(parents=True)
            source = root / 'examples/CppMemoryLinks.cpp'
            source.parent.mkdir()
            source.write_text('#include <packaged.h>\nint main() { return packaged(); }\n')
            installed = root / 'installed'
            headers = installed / 'lib/native/include'
            headers.mkdir(parents=True)
            (headers / 'packaged.h').write_text('inline int packaged() { return 0; }\n')
            consumer = root / 'consumer'
            consumer.mkdir()
            command = ['c++', '-I' + str(root / 'cpp/Platform.Data.Doublets'),
                       '-std=c++20', '-c', str(source), '-o', 'test.o']
            (build / 'compile_commands.json').write_text(json.dumps([
                {'command': shlex.join(command), 'file': str(source)}]))
            (target / 'link.txt').write_text(
                'c++ -Wl,--dependency-file=CMakeFiles/Platform.Data.Doublets.Tests.dir/link.d '
                '-Wl,--as-needed test.o -lm -o bin/tests\n')
            with patch.object(smoke_packages, 'ROOT', root):
                smoke_packages.compile_native_consumer(installed, consumer, build)
            self.assertTrue((consumer / 'native-consumer').is_file())
            self.assertFalse((consumer / 'CMakeFiles').exists())


if __name__ == '__main__':
    unittest.main()
