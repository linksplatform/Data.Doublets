"""Keep actionlint's checks while validating its known queue-key false positive.

actionlint 1.7.12 lacks GitHub's concurrency.queue schema (upstream #657).
Validate that field here and exclude only that exact obsolete-schema diagnostic.
Every other syntax, expression and embedded-shell diagnostic remains fatal.
"""

import argparse
import os
from pathlib import Path
from shutil import which
import subprocess
import yaml

IMAGE = "rhysd/actionlint@sha256:b1934ee5f1c509618f2508e6eb47ee0d3520686341fec936f3b79331f9315667"


def check_tools():
    for tool in ("shellcheck", "pyflakes"):
        if which(tool) is None:
            raise RuntimeError(
                f"{tool} is required: actionlint otherwise silently skips embedded code checks. Use --container or install the documented tools."
            )


def validate_queue(workflow):
    scopes = [workflow, *workflow.get("jobs", {}).values()]
    for scope in scopes:
        concurrency = scope.get("concurrency", {})
        if not isinstance(concurrency, dict) or "queue" not in concurrency:
            continue
        if concurrency["queue"] not in ("single", "max"):
            raise ValueError("concurrency.queue must be single or max")
        if (
            concurrency["queue"] == "max"
            and concurrency.get("cancel-in-progress", False) is not False
        ):
            raise ValueError("queue: max requires cancel-in-progress: false")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--container", action="store_true", help="Use the pinned image with ShellCheck and Pyflakes"
    )
    args = parser.parse_args()
    for path in Path(".github/workflows").glob("*.yml"):
        validate_queue(yaml.safe_load(path.read_text()))
    if args.container:
        command = ["docker", "run", "--rm", "-v", f"{Path.cwd()}:/work", "-w", "/work", IMAGE]
    else:
        check_tools()
        command = [os.environ.get("ACTIONLINT", "actionlint")]
    subprocess.run(
        [
            *command,
            "-color",
            "-ignore",
            '^unexpected key "queue" for "concurrency" section\\.',
            *map(str, Path(".github/workflows").glob("*.yml")),
        ],
        check=True,
    )
