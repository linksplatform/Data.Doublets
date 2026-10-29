"""Archive downloaded CI artifacts and verify every file against a SHA256 manifest.

Usage: python experiments/archive_ci_artifacts.py DOWNLOAD_DIRECTORY OUTPUT_PREFIX
The prefix produces .tar.gz and -manifest.json files in the evidence directory.
"""

import hashlib
import json
from pathlib import Path
import sys
import tarfile


def archive(source, prefix):
    source, prefix = Path(source), Path(prefix)
    files = sorted(path for path in source.rglob('*') if path.is_file())
    if not files:
        raise ValueError(f'No downloaded artifacts found in {source}')
    records = []
    destination = Path(str(prefix) + '.tar.gz')
    destination.parent.mkdir(parents=True, exist_ok=True)
    with tarfile.open(destination, 'w:gz') as output:
        for path in files:
            name = path.relative_to(source).as_posix()
            data = path.read_bytes()
            records.append(dict(path=name, size=len(data), sha256=hashlib.sha256(data).hexdigest()))
            output.add(path, arcname=name, recursive=False)
    with tarfile.open(destination, 'r:gz') as saved:
        assert set(saved.getnames()) == {record['path'] for record in records}
        for record in records:
            data = saved.extractfile(record['path']).read()
            assert len(data) == record['size'], record['path']
            assert hashlib.sha256(data).hexdigest() == record['sha256'], record['path']
    Path(str(prefix) + '-manifest.json').write_text(json.dumps(records, indent=2) + '\n')
    print(f'Byte-verified {len(records)} artifact files; archive size {destination.stat().st_size} bytes')


if __name__ == '__main__':
    archive(*sys.argv[1:])
