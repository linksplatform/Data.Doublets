"""Check future deployment URLs against the artifact, before deployment exists."""
from pathlib import Path
import os
import sys
import tempfile
import unittest
from unittest.mock import patch
sys.path.insert(0, str(Path(__file__).resolve().parents[1] / 'scripts'))
import check_docs


class GeneratedDocsTests(unittest.TestCase):
    def fixture(self, root):
        for name, data in {
            'csharp/api/Platform.Data.Doublets.html': b'<html></html>',
            'csharp/Platform.Data.Doublets.pdf': b'%PDF-fixture',
            'cpp/index.html': b'<html></html>',
        }.items():
            path = root / '_site' / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)

    def test_existing_future_page_passes_without_network(self):
        self.run_check('csharp/api/Platform.Data.Doublets.html')

    def test_removed_deployment_page_fails_even_if_still_live(self):
        with self.assertRaisesRegex(AssertionError, 'generated deployment target missing'):
            self.run_check('csharp/api/Removed.html')

    def run_check(self, target):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            self.fixture(root)
            (root / 'README.md').write_text(f'[API](https://linksplatform.github.io/Data.Doublets/{target})')
            previous = Path.cwd()
            try:
                os.chdir(root)
                with patch.object(check_docs, 'ROOT', root / '_site'):
                    check_docs.main()
            finally:
                os.chdir(previous)


if __name__ == '__main__':
    unittest.main()
