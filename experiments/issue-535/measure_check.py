"""Run a repository check and record child peak RSS within the container budget."""
import resource
import subprocess  # nosec B404 - Measure an explicitly selected local check.
import sys

result = subprocess.run(sys.argv[1:], check=False)  # nosec B603 - Local CLI utility; the caller selects the command, without a shell.
print(f'Child peak RSS: {resource.getrusage(resource.RUSAGE_CHILDREN).ru_maxrss} KiB', flush=True)
raise SystemExit(result.returncode)
