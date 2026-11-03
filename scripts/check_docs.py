"""Check generated documentation, local links, and the source PDF before upload."""

from html.parser import HTMLParser
from pathlib import Path
import re
from urllib.parse import unquote, urlsplit

ROOT = Path("_site")


class Links(HTMLParser):
    def __init__(self):
        super().__init__()
        self.links = []

    def handle_starttag(self, tag, attrs):
        self.links.extend(value for key, value in attrs if key in ("href", "src") and value)


def main():
    pages = list(ROOT.rglob("*.html"))
    assert pages, "DocFX did not generate HTML"
    assert (ROOT / "csharp/api/Platform.Data.Doublets.html").is_file(), "API reference is missing"
    assert (ROOT / "cpp/index.html").is_file(), "C++ API reference is missing"
    pdf = ROOT / "csharp/Platform.Data.Doublets.pdf"
    assert pdf.read_bytes().startswith(b"%PDF-"), "Source PDF is missing or invalid"
    errors = []
    for page in pages:
        parser = Links()
        parser.feed(page.read_text(encoding="utf-8"))
        for link in parser.links:
            url = urlsplit(link)
            if url.scheme or url.netloc or not url.path:
                continue
            if url.path.startswith("/Data.Doublets/"):
                target = ROOT / unquote(url.path.removeprefix("/Data.Doublets/"))
            elif url.path.startswith("/"):
                errors.append(f"{page}: unsupported root link {link}")
                continue
            else:
                target = page.parent / unquote(url.path)
            if not target.exists():
                errors.append(f"{page}: missing {link}")
    # Validate our deployment URLs against what this PR will actually publish.
    # Checking the old live deployment would reject newly fixed pages before
    # they can be deployed, and miss a removed page that still exists online.
    for markdown in Path(".").glob("*.md"):
        for link in re.findall(
            r"https://linksplatform\.github\.io/Data\.Doublets/[^\s)>]+", markdown.read_text()
        ):
            target = ROOT / unquote(urlsplit(link).path.removeprefix("/Data.Doublets/"))
            if not target.exists():
                errors.append(f"{markdown}: generated deployment target missing: {link}")
    assert not errors, "\n".join(errors)
    print(f"Validated {len(pages)} HTML pages, local links and source PDF")


if __name__ == "__main__":
    main()
