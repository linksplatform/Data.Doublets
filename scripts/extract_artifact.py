"""Extract one integrity-checked raw download-artifact ZIP without unzip-stream.

Use download-artifact's skip-decompress input; its digest check stays enabled.
Reject ambiguous downloads, traversal and symlinks before writing any files.
"""

import argparse
from pathlib import Path, PurePosixPath
import stat
from zipfile import ZipFile


def extract(source: Path, destination: Path):
    files = list(source.iterdir())
    if len(files) != 1 or not files[0].is_file() or files[0].is_symlink():
        raise ValueError("Expected exactly one downloaded artifact ZIP")
    destination = destination.resolve()
    with ZipFile(files[0]) as archive:
        for entry in archive.infolist():
            path = PurePosixPath(entry.filename)
            if (
                not entry.filename
                or "\\" in entry.filename
                or path.is_absolute()
                or ".." in path.parts
                or any(":" in part for part in path.parts)
                or stat.S_ISLNK(entry.external_attr >> 16)
                or not (destination / entry.filename).resolve().is_relative_to(destination)
            ):
                raise ValueError(f"Unsafe artifact member: {entry.filename!r}")
        archive.extractall(destination)
        print(f"Extracted {len(archive.infolist())} artifact member(s) to {destination}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("destination", type=Path)
    args = parser.parse_args()
    extract(args.source, args.destination)
