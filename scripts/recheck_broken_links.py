#!/usr/bin/env python3
# Shared with link-foundation/python-ai-driven-development-pipeline-template;
# "issue #N" references below point to that repository.
"""Re-check the lychee failures where no host ever answered.

lychee's ``--max-retries`` cannot retry a connection reset during connect
(lycheeverse/lychee#2297: the error is classified by its phase, and the
connect phase is answered ``false``), so a healthy URL that answers a RST --
a normal event for a rate-limiting or load-shedding host seen from a CI
address range -- is reported as broken without a single retry. This script
asks those URLs again, outside lychee.

The rule that keeps this from hiding real breakage: a failure carrying a
status code means a host answered, and that answer is final -- a 404 is
never re-checked.

Environment variables:
    LYCHEE_OUTPUT: path to the lychee markdown report (default lychee/out.md)
    RECOVERED_OUTPUT: where to write the URLs the re-check found healthy
        (default lychee/recovered.txt)
    RECHECK_BUDGET_SECONDS: total wall-clock budget for the re-check
        (default 240; must expire before the job's 10-minute cap)
    RECHECK_WAIT_MS: initial wait between rounds; doubles every round
        (default 2000)

GitHub Actions outputs:
    all_recovered: 'true' when every unanswered link answered healthy on
        re-check. Consumers must test ``!= 'true'``, never ``== 'false'``:
        a skipped or crashed step leaves the output empty, and only the
        ``!=`` form fails safe.

Exit codes: 0 in every case. This script downgrades failures; it never
raises them, so a bug here cannot turn a green run red.
"""

from __future__ import annotations

import os
import re
import sys
import time
import urllib.error
import urllib.request
from dataclasses import dataclass
from pathlib import Path
from typing import Callable

BUDGET_SECONDS_DEFAULT = 240
INITIAL_WAIT_MS_DEFAULT = 2000
REQUEST_TIMEOUT_SECONDS = 30
# lychee's documented default --accept list and --user-agent; used when the
# workflow does not set the flags. tests/test_recheck_broken_links.py reads
# this file and the workflow together, so the two cannot drift apart.
ACCEPT_DEFAULT = "100..=103,200..=299"
USER_AGENT_DEFAULT = "lychee"
WORKFLOW_PATH = Path(".github/workflows/links.yml")

ENTRY_PATTERN = re.compile(
    r"^\s*(?:\*|-)\s+\[([^\]]+)\]\s+<?([^\s>|)]+)>?"
    r"(?:\s+\(at [^)]*\))?\s*\|?\s*(.*)$",
    re.MULTILINE,
)


@dataclass(frozen=True)
class LycheeFailure:
    """One failure entry from the lychee markdown report."""

    marker: str
    url: str
    detail: str
    answered: bool


@dataclass(frozen=True)
class StillBroken:
    """A URL the re-check could not recover."""

    url: str
    status: int | None
    reason: str


@dataclass(frozen=True)
class RecheckResult:
    """Outcome of re-asking the URLs that never answered lychee."""

    recovered: list[str]
    still_broken: list[StillBroken]


def parse_lychee_failures(content: str) -> list[LycheeFailure]:
    """Split the report into failures, marking the answered ones final.

    A failure is "answered" when a numeric status marker is present ([404])
    or the detail says "Rejected status code" -- a host answered, and the
    answer is final. Everything else ([ERROR], [TIMEOUT], [UNKNOWN]) is a
    failure where no host ever answered.
    """
    failures: list[LycheeFailure] = []
    for match in ENTRY_PATTERN.finditer(content):
        marker = match.group(1).strip()
        url = match.group(2).strip().rstrip(".,;!?")
        detail = match.group(3).strip()
        if not url:
            continue
        answered = bool(re.fullmatch(r"\d{3}", marker)) or bool(
            re.search(r"rejected status code", detail, re.IGNORECASE)
        )
        failures.append(LycheeFailure(marker, url, detail, answered))
    return failures


def parse_accept_ranges(spec: str) -> Callable[[int], bool]:
    """Build an "is this status accepted" predicate from a lychee --accept list.

    Accepts the lychee syntax ``100..=103,200..=299,429`` with stray spaces.
    """
    accepted: set[int] = set()
    for part in spec.split(","):
        trimmed = part.strip()
        if not trimmed:
            continue
        range_match = re.fullmatch(r"(\d+)\.\.=(\d+)", trimmed)
        if range_match:
            accepted.update(
                range(int(range_match.group(1)), int(range_match.group(2)) + 1)
            )
        elif re.fullmatch(r"\d{3}", trimmed):
            accepted.add(int(trimmed))
    return lambda status: status in accepted


def extract_lychee_request_options(workflow_text: str) -> tuple[str, str]:
    """Extract the --accept list and --user-agent the lychee step runs with.

    The re-check must judge a URL by the same rules the checker used, so a
    link lychee would have accepted is accepted here too.
    """
    accept = re.search(r"--accept[=\s]+[\"']?([^\s\"']+)[\"']?", workflow_text)
    user_agent = re.search(r"--user-agent[=\s]+[\"']?([^\s\"']+)[\"']?", workflow_text)
    return (
        accept.group(1) if accept else ACCEPT_DEFAULT,
        user_agent.group(1) if user_agent else USER_AGENT_DEFAULT,
    )


def default_fetch(url: str, user_agent: str) -> int:
    """HEAD one URL once and return its final status.

    Follows redirects like lychee; a rejected status (4xx/5xx) arrives as an
    HTTPError carrying the code, which is a host's answer, not a transport
    failure. Transport failures (reset, timeout, DNS) raise.
    """
    request = urllib.request.Request(
        url, method="HEAD", headers={"User-Agent": user_agent}
    )
    try:
        with urllib.request.urlopen(  # noqa: S310 - URL comes from the lychee report
            request, timeout=REQUEST_TIMEOUT_SECONDS
        ) as response:
            return response.status
    except urllib.error.HTTPError as error:
        return error.code


def recheck_unanswered(
    urls: list[str],
    *,
    accept: str,
    user_agent: str,
    budget_seconds: float = BUDGET_SECONDS_DEFAULT,
    initial_wait_ms: float = INITIAL_WAIT_MS_DEFAULT,
    fetch: Callable[[str, str], int] | None = None,
) -> RecheckResult:
    """Ask every URL once more, round-robin with a doubling wait.

    Runs until everything either answers accepted or the budget runs out.
    Any answer is final: an accepted status recovers the URL, a rejected
    status fails it for good, and only a URL that keeps refusing to answer
    is retried.
    """
    is_accepted = parse_accept_ranges(accept)
    ask = fetch or default_fetch
    deadline = time.monotonic() + budget_seconds
    wait_ms = initial_wait_ms

    recovered: list[str] = []
    rejected: list[StillBroken] = []
    pending = list(dict.fromkeys(urls))

    first_round = True
    while pending and time.monotonic() < deadline:
        if not first_round:
            if time.monotonic() + wait_ms / 1000 > deadline:
                break
            time.sleep(wait_ms / 1000)
            wait_ms *= 2
        first_round = False

        still_pending: list[str] = []
        for url in pending:
            try:
                status = ask(url, user_agent)
            except Exception:  # noqa: BLE001 - no answer this round, whatever the cause
                still_pending.append(url)
                continue
            if is_accepted(status):
                recovered.append(url)
            else:
                rejected.append(
                    StillBroken(
                        url, status, f"answered {status}, which lychee does not accept"
                    )
                )
        pending = still_pending

    return RecheckResult(
        recovered=recovered,
        still_broken=rejected
        + [
            StillBroken(url, None, "no answer within the re-check budget")
            for url in pending
        ],
    )


def set_github_output(name: str, value: str) -> None:
    """Publish a GitHub Actions step output when a sink is configured."""
    output_file = os.environ.get("GITHUB_OUTPUT")
    if output_file:
        with Path(output_file).open("a", encoding="utf-8") as stream:
            stream.write(f"{name}={value}\n")


def main() -> int:
    """Re-ask every unanswered URL in the configured lychee report."""
    lychee_output = Path(os.environ.get("LYCHEE_OUTPUT", "lychee/out.md"))
    recovered_output = Path(os.environ.get("RECOVERED_OUTPUT", "lychee/recovered.txt"))
    accept, user_agent = extract_lychee_request_options(
        WORKFLOW_PATH.read_text(encoding="utf-8")
    )
    failures = parse_lychee_failures(lychee_output.read_text(encoding="utf-8"))

    final = [
        failure
        for failure in failures
        if failure.answered
        or not failure.url.lower().startswith(("http://", "https://"))
    ]
    unanswered = [
        failure.url
        for failure in failures
        if not failure.answered
        and failure.url.lower().startswith(("http://", "https://"))
    ]
    print(
        f"Re-check: {len(failures)} lychee failure(s), {len(final)} answered and "
        f"final, {len(unanswered)} never got an answer"
    )
    if not unanswered:
        print("Re-check: nothing to re-ask.")
        return 0

    result = recheck_unanswered(
        unanswered,
        accept=accept,
        user_agent=user_agent,
        budget_seconds=float(
            os.environ.get("RECHECK_BUDGET_SECONDS", BUDGET_SECONDS_DEFAULT)
        ),
        initial_wait_ms=float(
            os.environ.get("RECHECK_WAIT_MS", INITIAL_WAIT_MS_DEFAULT)
        ),
    )

    for url in result.recovered:
        print(
            f"::notice::{url} never answered lychee but answers {accept} now "
            "-- not a broken link"
        )
    if result.recovered:
        recovered_output.write_text(
            "\n".join(result.recovered) + "\n", encoding="utf-8"
        )
    print(
        f"Re-check finished: {len(result.recovered)} recovered, "
        f"{len(result.still_broken)} still without an answer"
    )

    if not result.still_broken and len(result.recovered) == len(unanswered):
        set_github_output("all_recovered", "true")
    return 0


if __name__ == "__main__":
    # The re-check only ever downgrades failures, so any crash here must not
    # mask itself as a verdict: exit 0 in every case.
    try:
        sys.exit(main())
    except Exception as error:  # noqa: BLE001 - no crash may raise the gate
        print(f"Re-check crashed (treating as no recovery): {error}")
        sys.exit(0)
