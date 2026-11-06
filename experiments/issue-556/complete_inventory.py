"""Resolve every selected direct/transitive package and add missing central pins."""

import concurrent.futures
import json
from pathlib import Path
import re
import tomllib
import urllib.request
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "dev/log/issues/556/pulls/557/research"


def lookup(item):
    ecosystem, name, selected = item
    if ecosystem == "NuGet":
        url = f"https://api.nuget.org/v3-flatcontainer/{name.lower()}/index.json"
        data = json.load(urllib.request.urlopen(url))
        latest = [v for v in data["versions"] if "-" not in v][-1]
    elif ecosystem == "PyPI":
        url = f"https://pypi.org/pypi/{name}/json"
        data = json.load(urllib.request.urlopen(url))
        latest = data["info"]["version"]
    else:
        # Crates.io sparse index is the registry Cargo itself resolves.
        lower = name.lower()
        prefix = (
            "1"
            if len(lower) == 1
            else "2"
            if len(lower) == 2
            else "3/" + lower[0]
            if len(lower) == 3
            else lower[:2] + "/" + lower[2:4]
        )
        url = f"https://index.crates.io/{prefix}/{lower}"
        lines = urllib.request.urlopen(url).read().decode().splitlines()
        data = [json.loads(line) for line in lines]
        latest = [r["vers"] for r in data if "-" not in r["vers"] and not r["yanked"]][-1]
    return dict(ecosystem=ecosystem, name=name, selected=selected, latest=latest, source=url)


def main():
    items = set()
    for path in (ROOT / "csharp").rglob("packages.lock.json"):
        for deps in json.loads(path.read_text())["dependencies"].values():
            for name, data in deps.items():
                if "resolved" in data:
                    items.add(("NuGet", name, data["resolved"]))
    props = ROOT / "Directory.Packages.props"
    central = {
        x.attrib["Include"]: x.attrib["Version"] for x in ET.parse(props).iter("PackageVersion")
    }
    items.update(("NuGet", name, version) for name, version in central.items())
    items.add(
        (
            "NuGet",
            "docfx",
            json.loads((ROOT / ".config/dotnet-tools.json").read_text())["tools"]["docfx"][
                "version"
            ],
        )
    )
    for name, version in re.findall(
        r"^([\w.-]+)==([^\s]+)", (ROOT / "scripts/requirements.lock.txt").read_text(), re.M
    ):
        items.add(("PyPI", name, version))
    for path in [
        ROOT / "experiments/issue-359/RustParity/Cargo.lock",
        ROOT / "experiments/issue-519/Cargo.lock",
    ]:
        for package in tomllib.loads(path.read_text())["package"]:
            if package.get("source", "").startswith("registry+"):
                items.add(("Cargo", package["name"], package["version"]))
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        results = list(pool.map(lookup, sorted(items)))
    (OUT / "registry-versions-after.json").write_text(json.dumps(results, indent=2) + "\n")
    missing = {
        r["name"]: r["latest"]
        for r in results
        if r["ecosystem"] == "NuGet" and r["name"] not in central and r["name"] != "docfx"
    }
    if missing:
        text = props.read_text()
        text = text.replace(
            "  </ItemGroup>",
            "".join(
                f'    <PackageVersion Include="{name}" Version="{version}" />\n'
                for name, version in sorted(missing.items())
            )
            + "  </ItemGroup>",
        )
        props.write_text(text)
    print("Resolved", len(results), "Added pins", missing)
    print(
        "Behind latest:",
        [
            (r["ecosystem"], r["name"], r["selected"], r["latest"])
            for r in results
            if r["latest"] != r["selected"]
        ],
    )


if __name__ == "__main__":
    main()
