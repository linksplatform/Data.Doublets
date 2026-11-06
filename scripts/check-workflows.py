"""Validate current GitHub workflow syntax, including embedded shell and Python."""

import argparse
import os
from pathlib import Path
from shutil import which
import subprocess

# kjanat/actionlint 1.17.0 supports concurrency.queue and self-repository refs.
IMAGE = "data-doublets-workflow-lint:local"


def check_tools():
    for tool in ("shellcheck", "pyflakes"):
        if which(tool) is None:
            raise RuntimeError(
                f"{tool} is required: actionlint otherwise silently skips embedded code checks. Use --container or install the documented tools."
            )


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--container", action="store_true", help="Use the pinned image with ShellCheck and Pyflakes"
    )
    args = parser.parse_args()
    if args.container:
        subprocess.run(
            ["docker", "build", "-q", "-t", IMAGE, "-f", "scripts/workflow-lint/Dockerfile", "."],
            check=True,
        )
        command = ["docker", "run", "--rm", "-v", f"{Path.cwd()}:/work", "-w", "/work", IMAGE]
    else:
        check_tools()
        command = [os.environ.get("ACTIONLINT", "actionlint")]
    subprocess.run(
        [*command, "-color", *map(str, Path(".github/workflows").glob("*.yml"))], check=True
    )
