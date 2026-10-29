"""Restore packaged artifacts in isolated consumers; never test project references."""
import argparse
import os
from pathlib import Path
import subprocess
import tempfile
import xml.etree.ElementTree as ET
import zipfile
from release import ROOT, info


def run(*args, cwd):
    subprocess.run(args, cwd=cwd, check=True)


def smoke(language):
    package = info(language)
    feed = ROOT / 'artifacts/packages'
    archive = feed / f"{package['id']}.{package['version']}.nupkg"
    with zipfile.ZipFile(archive) as contents:
        names = contents.namelist()
        if language == 'csharp':
            assert any(n.startswith('lib/net8') and n.endswith('.dll') for n in names)
            assert any(n.startswith('lib/net8') and n.endswith('.xml') for n in names)
            assert 'README.md' in names and 'icon.png' in names
        else:
            headers = list((ROOT / 'cpp/Platform.Data.Doublets').rglob('*.h'))
            expected = {'lib/native/include/' + h.relative_to(ROOT / 'cpp/Platform.Data.Doublets').as_posix() for h in headers}
            assert expected <= set(names), 'Native headers are missing from package'
            for header in headers:
                name = 'lib/native/include/' + header.relative_to(ROOT / 'cpp/Platform.Data.Doublets').as_posix()
                assert contents.read(name) == header.read_bytes(), f'Packaged header differs: {header}'
            assert 'images/icon.png' in names
            assert f"build/native/{package['id']}.targets" in names
            ET.fromstring(contents.read(f"{package['id']}.nuspec"))
    if language == 'cpp':
        with zipfile.ZipFile(feed / f"platform.data.doublets_{package['version']}.zip") as contents:
            for header in headers:
                name = 'Platform.Data.Doublets/' + header.relative_to(ROOT / 'cpp/Platform.Data.Doublets').as_posix()
                assert contents.read(name) == header.read_bytes(), f'ZIP header differs: {header}'
            assert contents.read('Platform.Data.Doublets/LICENSE') == (ROOT / 'LICENSE').read_bytes()
    with tempfile.TemporaryDirectory() as temporary:
        consumer = Path(temporary)
        # A separate packages folder prevents a cached published package from
        # hiding a missing or invalid local artifact with the same version.
        config = ET.Element('configuration')
        sources = ET.SubElement(config, 'packageSources')
        ET.SubElement(sources, 'clear')
        ET.SubElement(sources, 'add', key='tested-artifacts', value=str(feed))
        ET.SubElement(sources, 'add', key='nuget.org', value='https://api.nuget.org/v3/index.json')
        ET.ElementTree(config).write(consumer / 'NuGet.Config')
        properties = '<TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType>' if language == 'csharp' else '<TargetFramework>native</TargetFramework><TargetFrameworkIdentifier>native</TargetFrameworkIdentifier><TargetFrameworkVersion>v0.0</TargetFrameworkVersion>'
        (consumer / 'Consumer.csproj').write_text(f'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>{properties}</PropertyGroup><ItemGroup><PackageReference Include="{package["id"]}" Version="{package["version"]}" /></ItemGroup></Project>')
        if language == 'csharp':
            (consumer / 'Program.cs').write_text('using Platform.Data.Doublets; using Platform.Data.Doublets.Memory.United.Generic; using Platform.Memory; using var memory = new HeapResizableDirectMemory(); using var links = new UnitedMemoryLinks<ulong>(memory); links.Create(null, null); if (links.Count(null) != 1UL) throw new System.Exception("Package consumer failed");')
        run('dotnet', 'restore', '--configfile', str(consumer / 'NuGet.Config'), '--packages', str(consumer / 'packages'), cwd=consumer)
        if language == 'csharp':
            run('dotnet', 'run', '-c', 'Release', '--no-restore', cwd=consumer)
        else:
            installed = consumer / 'packages' / package['id'].lower() / package['version']
            assert (installed / 'lib/native/include/Platform.Data.Doublets.h').is_file()
    print(f"Validated packaged consumer of {package['id']} {package['version']}")


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('language', choices=['csharp', 'cpp'])
    smoke(parser.parse_args().language)
