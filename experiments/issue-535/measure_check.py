"""Run a repository check and record child peak RSS within the container budget."""
import resource
import subprocess
import sys

result = subprocess.run(sys.argv[1:], check=False)
print(f'Child peak RSS: {resource.getrusage(resource.RUSAGE_CHILDREN).ru_maxrss} KiB', flush=True)
raise SystemExit(result.returncode)
