"""Resolve each action release tag to an immutable commit, preserving pin policy."""

import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "dev/log/issues/556/pulls/557/research"
rows = json.loads((OUT / "registry-versions-before.json").read_text())
updates = {}
for row in rows:
    if row["ecosystem"] != "GitHub" or "latest" not in row:
        continue
    name = row["name"]
    if name not in {
        "NuGet/login",
        "actions/checkout",
        "actions/download-artifact",
        "actions/setup-dotnet",
        "actions/setup-python",
        "actions/upload-artifact",
        "dependabot/fetch-metadata",
        "github/codeql-action",
        "lycheeverse/lychee-action",
        "zizmorcore/zizmor-action",
    }:
        continue
    tag = "v4.39.3" if name == "github/codeql-action" else row["latest"]
    data = json.loads(subprocess.check_output(["gh", "api", f"repos/{name}/git/ref/tags/{tag}"]))
    obj = data["object"]
    while obj["type"] == "tag":
        obj = json.loads(
            subprocess.check_output(["gh", "api", f"repos/{name}/git/tags/{obj['sha']}"])
        )["object"]
    updates[name] = {"tag": tag, "sha": obj["sha"]}
for path in (ROOT / ".github/workflows").glob("*.yml"):
    text = path.read_text()
    for name, data in updates.items():
        text = re.sub(
            r"(uses: " + re.escape(name) + r"(?:/[\w.-]+)?@)[\da-f]+(?: # [^\n]+)?",
            lambda m, data=data: m[1] + data["sha"] + " # " + data["tag"],
            text,
        )
    text = (
        text.replace("python-version: '3.12'", "python-version: '3.15.2'")
        .replace("version: v1.29.0", "version: v1.32.0")
        .replace("lycheeVersion: v0.23.0", "lycheeVersion: v0.24.2")
    )
    path.write_text(text)
(OUT / "action-pins-after.json").write_text(json.dumps(updates, indent=2) + "\n")
