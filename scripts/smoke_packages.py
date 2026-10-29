"""Restore packaged artifacts in isolated consumers; never test project references."""

import argparse
import json
from pathlib import Path
import shlex
import subprocess
import tempfile
import urllib.parse
import xml.etree.ElementTree as ET
import zipfile
from release import NAMESPACE, ROOT, info


def run(*args, cwd):
    subprocess.run(args, cwd=cwd, check=True)


def validate_native_metadata(contents, package_id):
    manifest = ET.fromstring(contents.read(f"{package_id}.nuspec"))  # nosec B314 - Locally generated package metadata, not untrusted XML.
    license_element = manifest.find("n:metadata/n:license", NAMESPACE)
    if (
        license_element is None
        or license_element.get("type") != "expression"
        or not license_element.text
    ):
        raise ValueError("Native package must retain its SPDX license expression")
    # NuGet.org enforces this compatibility URL even though licenseUrl is
    # deprecated. Our ZIP packer does not get NuGet pack's normalization.
    expected_url = "https://licenses.nuget.org/" + urllib.parse.quote(license_element.text, safe="")
    if manifest.findtext("n:metadata/n:licenseUrl", namespaces=NAMESPACE) != expected_url:
        raise ValueError("Native package licenseUrl must match its SPDX license expression")
    if contents.read("LICENSE") != (ROOT / "LICENSE").read_bytes():
        raise ValueError("Native package must retain the bundled license file")


def compile_native_consumer(installed, consumer, build_dir):
    database = json.loads((build_dir / "compile_commands.json").read_text())
    if not database:
        raise ValueError("A real native compilation database is required for the package consumer")
    entry = database[0]
    original = shlex.split(entry["command"])
    command = [original[0]]
    replaced = False
    index = 1
    while index < len(original):
        argument = original[index]
        if argument == "-o":
            index += 2
            continue
        if argument not in ("-c", entry["file"]):
            source_include = str(ROOT / "cpp/Platform.Data.Doublets")
            if source_include in argument:
                argument = argument.replace(source_include, str(installed / "lib/native/include"))
                replaced = True
            command.append(argument)
        index += 1
    if not replaced:
        raise ValueError("Cannot isolate the native package from source-tree headers")
    linker = shlex.split(
        (build_dir / "CMakeFiles/Platform.Data.Doublets.Tests.dir/link.txt").read_text()
    )
    # CMake's dependency output belongs to the original build directory. Reusing
    # it from the isolated consumer fails when that relative directory is absent.
    libraries = [
        arg
        for arg in linker
        if (arg.endswith((".a", ".so")) or arg.startswith(("-l", "-L", "-Wl,")))
        and "gtest" not in arg
        and not arg.startswith("-Wl,--dependency-file=")
    ]
    binary = consumer / "native-consumer"
    command += [str(ROOT / "examples/CppMemoryLinks.cpp"), *libraries, "-o", str(binary)]
    run(*command, cwd=consumer)
    run(str(binary), cwd=consumer)


def smoke(language, build_dir=None):
    headers: list[Path] = []
    package = info(language)
    feed = ROOT / "artifacts/packages"
    archive = feed / f"{package['id']}.{package['version']}.nupkg"
    with zipfile.ZipFile(archive) as contents:
        names = contents.namelist()
        if language == "csharp":
            assert any(n.startswith("lib/net8") and n.endswith(".dll") for n in names)
            assert any(n.startswith("lib/net8") and n.endswith(".xml") for n in names)
            assert "README.md" in names and "icon.png" in names
        else:
            headers = list((ROOT / "cpp/Platform.Data.Doublets").rglob("*.h"))
            expected = {
                "lib/native/include/"
                + h.relative_to(ROOT / "cpp/Platform.Data.Doublets").as_posix()
                for h in headers
            }
            assert expected <= set(names), "Native headers are missing from package"
            for header in headers:
                name = (
                    "lib/native/include/"
                    + header.relative_to(ROOT / "cpp/Platform.Data.Doublets").as_posix()
                )
                assert contents.read(name) == header.read_bytes(), (
                    f"Packaged header differs: {header}"
                )
            assert "images/icon.png" in names
            assert f"build/native/{package['id']}.targets" in names
            validate_native_metadata(contents, package["id"])
    if language == "cpp":
        with zipfile.ZipFile(feed / f"platform.data.doublets_{package['version']}.zip") as contents:
            for header in headers:
                name = (
                    "Platform.Data.Doublets/"
                    + header.relative_to(ROOT / "cpp/Platform.Data.Doublets").as_posix()
                )
                assert contents.read(name) == header.read_bytes(), f"ZIP header differs: {header}"
            assert (
                contents.read("Platform.Data.Doublets/LICENSE") == (ROOT / "LICENSE").read_bytes()
            )
    with tempfile.TemporaryDirectory() as temporary:
        consumer = Path(temporary)
        # A separate packages folder prevents a cached published package from
        # hiding a missing or invalid local artifact with the same version.
        config = ET.Element("configuration")
        sources = ET.SubElement(config, "packageSources")
        ET.SubElement(sources, "clear")
        ET.SubElement(sources, "add", key="tested-artifacts", value=str(feed))
        ET.SubElement(sources, "add", key="nuget.org", value="https://api.nuget.org/v3/index.json")
        ET.ElementTree(config).write(consumer / "NuGet.Config")
        properties = (
            "<TargetFramework>net8.0</TargetFramework><OutputType>Exe</OutputType>"
            if language == "csharp"
            else "<TargetFramework>native</TargetFramework><TargetFrameworkIdentifier>native</TargetFrameworkIdentifier><TargetFrameworkVersion>v0.0</TargetFrameworkVersion>"
        )
        (consumer / "Consumer.csproj").write_text(
            f'<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>{properties}</PropertyGroup><ItemGroup><PackageReference Include="{package["id"]}" Version="{package["version"]}" /></ItemGroup></Project>'
        )
        if language == "csharp":
            (consumer / "Program.cs").write_text(
                'using Platform.Data.Doublets; using Platform.Data.Doublets.Memory.United.Generic; using Platform.Memory; using var memory = new HeapResizableDirectMemory(); using var links = new UnitedMemoryLinks<ulong>(memory); links.Create(null, null); if (links.Count(null) != 1UL) throw new System.Exception("Package consumer failed");'
            )
        run(
            "dotnet",
            "restore",
            "--configfile",
            str(consumer / "NuGet.Config"),
            "--packages",
            str(consumer / "packages"),
            cwd=consumer,
        )
        if language == "csharp":
            run("dotnet", "run", "-c", "Release", "--no-restore", cwd=consumer)
        else:
            installed = consumer / "packages" / package["id"].lower() / package["version"]
            assert (installed / "lib/native/include/Platform.Data.Doublets.h").is_file()
            compile_native_consumer(installed, consumer, build_dir or ROOT / "cpp/build")
    print(f"Validated packaged consumer of {package['id']} {package['version']}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("language", choices=["csharp", "cpp"])
    parser.add_argument("--build-dir", type=Path)
    args = parser.parse_args()
    smoke(args.language, args.build_dir)
