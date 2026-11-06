"""Compile the registry snapshots and baseline manifests into reviewable tables."""

import json
from pathlib import Path
import subprocess
import tomllib

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "dev/log/issues/556/pulls/557/research"
BASE = "78088724dca579aea0d5479a7e4935bba8f0a123"


def baseline(path):
    return subprocess.check_output(["git", "show", f"{BASE}:{path}"], text=True)


def main():
    before = json.loads((OUT / "registry-versions-before.json").read_text())
    after = json.loads((OUT / "registry-versions-after.json").read_text())
    old = {}
    for row in before:
        old.setdefault((row["ecosystem"], row["name"].lower()), set()).add(row["old"])
    for path in ["experiments/issue-359/RustParity/Cargo.lock", "experiments/issue-519/Cargo.lock"]:
        for item in tomllib.loads(baseline(path))["package"]:
            old.setdefault(("Cargo", item["name"].lower()), set()).add(item["version"])
    exceptions = {
        "Platform.Collections.Methods": "Published 1.0.0 lacks Methods.Trees/Lists APIs used by this library; minimal compile regression and upstream #114. Retain 0.7.3.",
        "Perfolizer": "BenchmarkDotNet 0.15.8 requires exactly [0.6.1]; 0.7.5 violates upstream constraint. BDN #3316.",
        "Pragmastat": "Perfolizer 0.6.1 calls removed MeasurementUnit constructor in 14.0.1; benchmark execution fails. BDN #3316.",
        "platform-data": "Current doublets 0.20.0 requires v2; v3 needs upstream trait/API migration, reported upstream.",
        "r-efi": "Current getrandom 0.4.3 declares r-efi ^6 for UEFI; 7.1 cannot satisfy its supported API requirement.",
        "windows-link": "Current windows-sys declares windows-link ^0.2; 0.100 cannot satisfy its supported API requirement.",
    }
    lines = [
        "# Complete dependency inventory",
        "",
        "Registry snapshots were resolved on 2026-11-05 UTC. “Before” is the prepared baseline; “new/unlocked” means no exact baseline lock existed. Stable releases are selected; preview releases are excluded. Rows include development and transitive packages. The six constrained entries have explicit upstream reasons; they are not silently described as upgraded. Raw registry URLs and selected versions are retained in registry-versions-after.json.",
        "",
    ]
    for ecosystem in ["NuGet", "PyPI", "Cargo"]:
        lines += [
            f"## {ecosystem}",
            "",
            "| Dependency | Before | Latest released | Selected | Reason / registry |",
            "|---|---|---|---|---|",
        ]
        for row in after:
            if row["ecosystem"] != ecosystem:
                continue
            previous = ", ".join(
                sorted(old.get((ecosystem, row["name"].lower()), {"new/unlocked"}))
            )
            reason = exceptions.get(row["name"], "Current stable")
            lines.append(
                f"| {row['name']} | {previous} | {row['latest']} | {row['selected']} | {reason}; [registry]({row['source']}) |"
            )
        if ecosystem == "NuGet":
            remaining = {r["name"].lower() for r in after if r["ecosystem"] == ecosystem}
            removed = [
                r
                for r in before
                if r["ecosystem"] == ecosystem and r["name"].lower() not in remaining
            ]
            lines += [
                "",
                "Packages removed by the .NET 10 / xUnit v3 migration (no obsolete compatibility package is added back):",
                "",
                "| Package | Before | Registry latest | Result |",
                "|---|---|---|---|",
            ]
            seen = set()
            for row in removed:
                if row["name"] in seen:
                    continue
                seen.add(row["name"])
                lines.append(
                    f"| {row['name']} | {', '.join(sorted(old[(ecosystem, row['name'].lower())]))} | {row['latest']} | Removed; [registry]({row['source']}) |"
                )
        lines.append("")
    lines += [
        "## GitHub Actions",
        "",
        "| Action | Before SHA | Latest / selected release | Selected SHA |",
        "|---|---|---|---|",
    ]
    for name, row in json.loads((OUT / "action-pins-after.json").read_text()).items():
        previous = ", ".join(sorted(old.get(("GitHub", name.lower()), {"new"})))
        lines.append(
            f"| [{name}](https://github.com/{name}/releases/tag/{row['tag']}) | {previous} | {row['tag']} | {row['sha']} |"
        )
    lines += [
        "",
        "CodeQL releases/latest points to a **bundle**, not an Action version. Action v4.39.3 is resolved from the actual action release/tag; the initial before-snapshot bundle result is superseded here.",
        "",
    ]
    (OUT / "DEPENDENCIES.md").write_text("\n".join(lines))


if __name__ == "__main__":
    main()
