"""Check registry-required metadata in the actual generated native package."""

from pathlib import Path
import io
import sys
import tempfile
import unittest
import xml.etree.ElementTree as ET
from zipfile import ZipFile

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'scripts'))
import release
import smoke_packages


class NativeLicenseTests(unittest.TestCase):
    def test_generated_package_retains_spdx_and_legacy_license_metadata(self):
        with tempfile.TemporaryDirectory() as directory:
            output = Path(directory)
            release.pack_cpp(output)
            package = release.info('cpp')
            with ZipFile(output / f"{package['id']}.{package['version']}.nupkg") as archive:
                manifest = ET.fromstring(archive.read(f"{package['id']}.nuspec"))
                license_element = manifest.find('n:metadata/n:license', release.NAMESPACE)
                self.assertIsNotNone(license_element)
                self.assertEqual(license_element.attrib, {'type': 'expression'})
                self.assertEqual(license_element.text, 'LGPL-3.0-only')
                self.assertEqual(
                    manifest.findtext('n:metadata/n:licenseUrl', namespaces=release.NAMESPACE),
                    'https://licenses.nuget.org/LGPL-3.0-only',
                )
                self.assertEqual(archive.read('LICENSE'), (ROOT / 'LICENSE').read_bytes())
                smoke_packages.validate_native_metadata(archive, package['id'])

    def test_consumer_rejects_missing_or_mismatched_compatibility_metadata(self):
        for license_xml, url in (
            ('<license type="expression">LGPL-3.0-only</license>', ''),
            ('<license type="expression">LGPL-3.0-only</license>',
             '<licenseUrl>https://licenses.nuget.org/MIT</licenseUrl>'),
            ('', '<licenseUrl>https://licenses.nuget.org/LGPL-3.0-only</licenseUrl>'),
            ('<license type="file">LICENSE</license>',
             '<licenseUrl>https://licenses.nuget.org/LGPL-3.0-only</licenseUrl>'),
        ):
            with self.subTest(license_xml=license_xml, url=url), io.BytesIO() as buffer:
                with ZipFile(buffer, 'w') as archive:
                    archive.writestr('Example.nuspec',
                                     f'<package xmlns="{release.NAMESPACE["n"]}">'
                                     f'<metadata>{license_xml}{url}</metadata></package>')
                    archive.writestr('LICENSE', (ROOT / 'LICENSE').read_bytes())
                with ZipFile(buffer) as archive:
                    with self.assertRaisesRegex(ValueError, 'SPDX license expression'):
                        smoke_packages.validate_native_metadata(archive, 'Example')

    def test_consumer_rejects_changed_bundled_license(self):
        with io.BytesIO() as buffer:
            with ZipFile(buffer, 'w') as archive:
                archive.writestr('Example.nuspec',
                                 f'<package xmlns="{release.NAMESPACE["n"]}"><metadata>'
                                 '<license type="expression">LGPL-3.0-only</license>'
                                 '<licenseUrl>https://licenses.nuget.org/LGPL-3.0-only</licenseUrl>'
                                 '</metadata></package>')
                archive.writestr('LICENSE', 'wrong license')
            with ZipFile(buffer) as archive:
                with self.assertRaisesRegex(ValueError, 'bundled license file'):
                    smoke_packages.validate_native_metadata(archive, 'Example')


if __name__ == '__main__':
    unittest.main()
