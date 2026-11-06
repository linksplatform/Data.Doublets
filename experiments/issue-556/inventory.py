"""Resolve live dependency coordinates from registries before modifying manifests."""

import concurrent.futures
import json
from pathlib import Path
import re
import subprocess
import urllib.request

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "dev/log/issues/556/pulls/557/research"


def get(url):
    with urllib.request.urlopen(url) as response:
        return json.load(response)


def resolve(item):
    ecosystem, name, old = item
    try:
        if ecosystem == "NuGet":
            url = f"https://api.nuget.org/v3-flatcontainer/{name.lower()}/index.json"
            versions = get(url)["versions"]
            stable = [v for v in versions if "-" not in v]
            latest = stable[-1]
        elif ecosystem == "PyPI":
            url = f"https://pypi.org/pypi/{name}/json"
            data = get(url)
            latest = data["info"]["version"]
            (OUT / f"pypi-{name}.json").write_text(
                json.dumps({"info": data["info"], "urls": data["urls"]}, indent=2) + "\n"
            )
        elif ecosystem == "GitHub":
            url = f"https://api.github.com/repos/{name}/releases/latest"
            data = json.loads(
                subprocess.check_output(["gh", "api", f"repos/{name}/releases/latest"])
            )
            latest = data["tag_name"]
            (OUT / ("github-" + name.replace("/", "-") + ".json")).write_text(
                json.dumps(data, indent=2) + "\n"
            )
        else:
            raise ValueError(f"Unsupported ecosystem: {ecosystem}")
        return dict(ecosystem=ecosystem, name=name, old=old, latest=latest, source=url)
    except Exception as error:
        return dict(ecosystem=ecosystem, name=name, old=old, error=str(error))


def main():
    items = set()
    for path in ROOT.rglob("*.csproj"):
        if "dev" not in path.parts:
            for name, old in re.findall(
                r'<PackageReference Include="([^"]+)" Version="([^"]+)"', path.read_text()
            ):
                items.add(("NuGet", name, old))
    for path in (ROOT / "csharp").rglob("packages.lock.json"):
        for deps in json.loads(path.read_text())["dependencies"].values():
            for name, data in deps.items():
                if "resolved" in data:
                    items.add(("NuGet", name, data["resolved"]))
    items.add(("NuGet", "docfx", "2.81.0"))
    for path in (ROOT / "scripts").glob("requirements*.txt"):
        for name, old in re.findall(r"^([\w.-]+)==([^\s]+)", path.read_text(), re.M):
            items.add(("PyPI", name, old))
    for path in (ROOT / ".github/workflows").glob("*.yml"):
        for name, old in re.findall(
            r"uses: ([\w.-]+/[\w.-]+)(?:/[\w.-]+)?@([\da-f]+)", path.read_text()
        ):
            items.add(("GitHub", name, old))
    for name, old in [
        ("gitleaks/gitleaks", "v8.30.1"),
        ("rhysd/actionlint", "v1.7.12"),
        ("zizmorcore/zizmor", "v1.29.0"),
        ("lycheeverse/lychee", "v0.23.0"),
        ("google/googletest", "v1.13.0"),
        ("google/benchmark", "v1.6.0"),
        ("microsoft/GSL", "v4.0.0"),
        ("Kitware/CMake", "3.13"),
        ("python/cpython", "3.12"),
        ("rust-lang/rust", "1.99.0"),
    ]:
        items.add(("GitHub", name, old))
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        results = list(pool.map(resolve, sorted(items)))
    (OUT / "registry-versions-before.json").write_text(json.dumps(results, indent=2) + "\n")
    lines = [
        "# Registry inventory before updates",
        "",
        "| Ecosystem | Dependency | Previous pin | Latest stable | Registry |",
        "|---|---|---|---|---|",
    ]
    for row in results:
        lines.append(
            "| {ecosystem} | {name} | {old} | {latest} | {source} |".format(
                latest=row.get("latest", row.get("error")),
                source=row.get("source", "unresolved"),
                **{k: v for k, v in row.items() if k not in ("latest", "source")},
            )
        )
    (OUT / "DEPENDENCIES.md").write_text("\n".join(lines) + "\n")
    print(f"Resolved {len(results)} coordinates; errors: {sum('error' in r for r in results)}")


if __name__ == "__main__":
    main()
