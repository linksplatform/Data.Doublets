#!/usr/bin/env python3
# Shared with link-foundation/python-ai-driven-development-pipeline-template;
# "issue #N" references below point to that repository.
"""Check broken links for snapshots in the Wayback Machine."""

from __future__ import annotations

import json
import os
import re
import sys
import time
import urllib.parse
import urllib.request
from dataclasses import dataclass
from pathlib import Path
from typing import Any

WAYBACK_API = "https://archive.org/wayback/available?url="
DEFAULT_LYCHEE_OUTPUT = Path("lychee/out.md")
REQUEST_TIMEOUT_SECONDS = 10
REQUEST_DELAY_SECONDS = 0.5
USER_AGENT = "broken-link-checker/1.0 (GitHub Actions CI)"

STATUS_URL_PATTERN = re.compile(
    r"\[(?:4\d\d|5\d\d|ERROR|TIMEOUT|UNKNOWN)\]\s+" r"(https?://[^\s)]+)",
    re.IGNORECASE,
)
BULLET_URL_PATTERN = re.compile(
    r"^\s*(?:\*|-)\s+.*?(https?://[^\s|)>\]]+)", re.MULTILINE
)


@dataclass(frozen=True)
class ArchiveResult:
    """Availability details for one Wayback Machine snapshot."""

    available: bool
    archive_url: str | None = None
    timestamp: str | None = None


def extract_broken_urls(content: str) -> list[str]:
    """Extract and deduplicate broken HTTP URLs from a lychee Markdown report."""
    # Newer lychee reports list followed redirects (and suggestions) in their
    # own "## ... per input" sections; only the errors are broken links.
    content = re.sub(
        r"^## (?!Errors per input)[^\n]*\n(?:(?!## )[^\n]*\n?)*",
        "",
        content,
        flags=re.MULTILINE,
    )
    urls: list[str] = []
    for pattern in (STATUS_URL_PATTERN, BULLET_URL_PATTERN):
        for match in pattern.finditer(content):
            url = match.group(1).strip().rstrip(".,;!?")
            if url not in urls:
                urls.append(url)
    return urls


def split_recovered_urls(
    urls: list[str], recovered_text: str | None
) -> tuple[list[str], list[str]]:
    """Drop URLs the link re-check found healthy from the archive lookup.

    A URL that never answered lychee but answers the re-check is not a broken
    link; keeping it in this report would send a healthy URL to the Wayback
    Machine and fail the job on it.
    """
    recovered_set = {
        line.strip() for line in (recovered_text or "").splitlines() if line.strip()
    }
    remaining = [url for url in urls if url not in recovered_set]
    recovered = [url for url in urls if url in recovered_set]
    return remaining, recovered


def fetch_json(url: str) -> dict[str, Any]:
    """Fetch a JSON object with a bounded Wayback Machine request."""
    request = urllib.request.Request(url, headers={"User-Agent": USER_AGENT})
    with urllib.request.urlopen(  # noqa: S310 - URL targets a fixed HTTPS API
        request, timeout=REQUEST_TIMEOUT_SECONDS
    ) as response:
        payload = json.load(response)
    if not isinstance(payload, dict):
        msg = "Wayback Machine returned a non-object JSON response"
        raise ValueError(msg)
    return payload


def check_wayback_machine(url: str) -> ArchiveResult:
    """Return the closest available Wayback Machine snapshot for ``url``."""
    api_url = f"{WAYBACK_API}{urllib.parse.quote(url, safe='')}"
    try:
        payload = fetch_json(api_url)
        snapshots = payload.get("archived_snapshots", {})
        closest = snapshots.get("closest", {}) if isinstance(snapshots, dict) else {}
        if not isinstance(closest, dict) or closest.get("available") is not True:
            return ArchiveResult(available=False)

        archive_url = closest.get("url")
        timestamp = closest.get("timestamp")
        if not isinstance(archive_url, str) or not isinstance(timestamp, str):
            return ArchiveResult(available=False)
        return ArchiveResult(
            available=True,
            archive_url=re.sub(r"^http://", "https://", archive_url),
            timestamp=timestamp,
        )
    except (OSError, TimeoutError, ValueError, json.JSONDecodeError) as error:
        print(f"  Failed to check Wayback Machine for {url}: {error}")
        return ArchiveResult(available=False)


def format_timestamp(timestamp: str | None) -> str:
    """Format a Wayback timestamp as YYYY-MM-DD when possible."""
    if timestamp is None or len(timestamp) < 8:
        return timestamp or "unknown date"
    return f"{timestamp[:4]}-{timestamp[4:6]}-{timestamp[6:8]}"


def set_output(name: str, value: str) -> None:
    """Publish a GitHub Actions output and echo it for local runs."""
    output_file = os.environ.get("GITHUB_OUTPUT")
    if output_file:
        with Path(output_file).open("a", encoding="utf-8") as stream:
            stream.write(f"{name}={value}\n")
    print(f"{name}={value}")


def report_archived(url: str, result: ArchiveResult) -> None:
    """Emit an actionable GitHub notice for an archived broken link."""
    date = format_timestamp(result.timestamp)
    print(f"  Archived on {date}: {result.archive_url}")
    print(
        f"::notice title=Broken link - Web Archive available ({date})::"
        f"Broken link detected: {url}\n"
        f"A Web Archive snapshot from {date} is available.\n"
        "Suggested fix: replace the broken link with the archived version:\n"
        f"  {result.archive_url}"
    )


def report_unarchived(url: str) -> None:
    """Emit an actionable GitHub error for an unrecoverable broken link."""
    print("  Not found in Web Archive")
    print(
        "::error title=Broken link - No Web Archive fallback::"
        f"Broken link detected: {url}\n"
        "No archived version was found in the Wayback Machine.\n"
        "Find an updated URL, remove the link, or add it to .lycheeignore "
        "if it is a known false positive."
    )


def main() -> int:
    """Check every URL in the configured lychee report."""
    output_path = Path(os.environ.get("LYCHEE_OUTPUT", DEFAULT_LYCHEE_OUTPUT))
    print("=== Web Archive Fallback Check ===")
    print(f"Reading lychee output from: {output_path}")

    if not output_path.exists():
        print("::error::Lychee output is missing; cannot verify broken links")
        set_output("all_archived", "false")
        return 1

    broken_urls = extract_broken_urls(output_path.read_text(encoding="utf-8"))
    recovered_file = Path(os.environ.get("RECOVERED_URLS", "lychee/recovered.txt"))
    recovered_text = (
        recovered_file.read_text(encoding="utf-8") if recovered_file.exists() else ""
    )
    broken_urls, rechecked_healthy = split_recovered_urls(broken_urls, recovered_text)
    for url in rechecked_healthy:
        print(f"  {url} never answered lychee but answers the re-check -- not broken")
    if not broken_urls:
        print("No broken URLs found in lychee output.")
        set_output("all_archived", "true")
        return 0

    unarchived: list[str] = []
    print(f"Found {len(broken_urls)} broken URL(s). Checking Web Archive...")
    for index, url in enumerate(broken_urls):
        print(f"Checking: {url}")
        result = check_wayback_machine(url)
        if result.available:
            report_archived(url, result)
        else:
            unarchived.append(url)
            report_unarchived(url)
        if index < len(broken_urls) - 1:
            time.sleep(REQUEST_DELAY_SECONDS)

    all_archived = not unarchived
    set_output("all_archived", "true" if all_archived else "false")
    if not all_archived:
        print("Action required: fix or remove the broken links listed above.")
        return 1
    print("All broken links have Web Archive versions.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
