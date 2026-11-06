"""Supplement the Conan lock audit by checking exact upstream source commits.

Conan aliases for private Platform recipes are not public package identities.
Resolve each recipe's GitHub source tag to its commit, including date versions.
Network failures and incomplete responses fail the audit.
"""

import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import re
import subprocess
import urllib.request


def source_commit(node):
    name, version = node["name"], node["version"]
    if name == "cmake" and node["recipe"] == "Platform":
        url = f"https://github.com/Kitware/CMake/archive/v{version}.tar.gz"
    else:
        data = node["conandata"]["sources"][version]
        url = data.get("source", data)["url"]
        if isinstance(url, list):
            url = url[0]
    match = re.fullmatch(
        r"https://github.com/([^/]+/[^/]+)/(?:archive/(?:refs/tags/)?(.+)\.(?:tar\.gz|zip)|releases/download/([^/]+)/[^/]+)",
        url,
    )
    if match is None:
        raise ValueError(f"Unsupported source URL: {url}")
    repository, archive, release = match.groups()
    ref = archive or release
    if re.fullmatch(r"[a-f0-9]{40}", ref):
        commit = ref
    else:
        commit = subprocess.check_output(
            ["gh", "api", f"repos/{repository}/commits/{ref}", "--jq", ".sha"], text=True
        ).strip()
    if not re.fullmatch(r"[a-f0-9]{40}", commit):
        raise ValueError(f"Invalid source commit for {node['ref']}")
    return dict(package=f"{name}/{version}", source=url, commit=commit)


def audit(graph):
    packages: dict[tuple[str, str], dict] = {}
    for node in graph["graph"]["nodes"].values():
        if node.get("name"):
            packages.setdefault((node["name"], node["version"]), node)
    if not packages:
        raise ValueError("Conan graph contains no packages")
    sources = [source_commit(node) for node in packages.values()]
    request = urllib.request.Request(
        "https://api.osv.dev/v1/querybatch",
        data=json.dumps({"queries": [{"commit": source["commit"]} for source in sources]}).encode(),
        headers={"Content-Type": "application/json"},
    )
    with urllib.request.urlopen(request) as response:
        results = json.load(response)["results"]
    if len(results) != len(sources):
        raise ValueError("Incomplete OSV response")
    for source, result in zip(sources, results, strict=True):
        if not isinstance(result, dict) or result.get("next_page_token"):
            raise ValueError("Incomplete OSV result; review pagination before accepting audit")
        source["vulnerabilities"] = result.get("vulns", [])
    return dict(checked_at=datetime.now(timezone.utc).isoformat(), sources=sources)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("graph", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    result = audit(json.loads(args.graph.read_text()))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2) + "\n")
    affected = [source["package"] for source in result["sources"] if source["vulnerabilities"]]
    if affected:
        raise SystemExit("OSV advisories found: " + ", ".join(affected))
    print(f"Audited {len(result['sources'])} native source commits: no known OSV advisories")


if __name__ == "__main__":
    main()
