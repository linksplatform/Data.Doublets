"""Measure a single-worker native build inside the caller's memory limit."""
import resource
import subprocess
import sys

result = subprocess.run(['cmake', '--build', sys.argv[1], '--parallel', '1'])
usage = resource.getrusage(resource.RUSAGE_CHILDREN)
print(f'Child peak RSS: {usage.ru_maxrss} KiB', flush=True)
raise SystemExit(result.returncode)
