"""Detect relevant changes using the event's actual base, including deleted files."""

import json
import os
from pathlib import Path
import subprocess


def categories(paths):
    result = dict(csharp=False, cpp=False, docs=False, pipeline=False)
    for path in paths:
        if path.startswith(("dev/log/", "experiments/")) or path.endswith(".gitkeep"):
            continue
        result["docs"] |= path.endswith(".md") or path.startswith("docs/")
        if path.endswith(".md"):
            continue
        result["csharp"] |= path.startswith(
            ("csharp/", "examples/ExternalReferences/", "examples/Deletion/", "examples/Update/")
        ) or path in ("global.json", "Directory.Build.props")
        result["cpp"] |= path.startswith(("cpp/", "c/")) or path in (
            "examples/CppMemoryLinks.cpp",
            ".clang-format",
            ".clang-tidy",
        )
        result["pipeline"] |= path.startswith((".github/", "scripts/", ".config/")) or path in (
            ".gitpod.yml",
            "publish-bin.sh",
            "docfx.json",
            ".editorconfig",
            ".gitattributes",
            ".lycheeignore",
            ".gitleaksignore",
            ".pre-commit-config.yaml",
            "pyproject.toml",
        )
    return result


def git(*args):
    return subprocess.check_output(["git", *args])


def changed_paths(event, event_name):
    if event_name == "pull_request":
        base = event["pull_request"]["base"]["sha"]
    else:
        base = event.get("before", "")
    if not base or set(base) == {"0"}:
        return git("ls-files", "-z").decode().strip("\0").split("\0")
    # Checkout fetches full history, and a missing base is an error, never an
    # empty diff that silently skips checks. --no-renames includes both paths.
    return (
        git("diff", "--no-renames", "--name-only", "-z", base, "HEAD")
        .decode()
        .strip("\0")
        .split("\0")
    )


if __name__ == "__main__":
    event_file = os.environ.get("GITHUB_EVENT_PATH")
    event = json.loads(Path(event_file).read_text()) if event_file else {}
    result = categories(
        changed_paths(event, os.environ.get("GITHUB_EVENT_NAME", "workflow_dispatch"))
    )
    output = "".join(f"{key}={str(value).lower()}\n" for key, value in result.items())
    print(output, end="")
    if os.environ.get("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as stream:
            stream.write(output)
