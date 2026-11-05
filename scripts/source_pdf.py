"""Produce the existing e-reader source PDF with Python 3 and embedded Unicode."""

import argparse
from pathlib import Path


def build_pdf(root, destination, font):
    from fpdf import FPDF
    from fpdf.enums import WrapMode

    pdf = FPDF()
    pdf.set_auto_page_break(auto=True, margin=10)
    pdf.add_font("DejaVu", fname=str(font))
    pdf.set_font("DejaVu", size=7)
    pdf.set_title("Platform.Data.Doublets C# source")
    for source in sorted((root / "csharp").rglob("*.cs")):
        if {"bin", "obj"}.intersection(source.parts):
            continue
        pdf.add_page()
        pdf.multi_cell(
            w=0, h=4, text=source.relative_to(root).as_posix(), new_x="LMARGIN", new_y="NEXT"
        )
        for line in source.read_text(encoding="utf-8-sig").splitlines():
            pdf.multi_cell(
                w=0,
                h=3,
                text=line.expandtabs(4) or " ",
                wrapmode=WrapMode.CHAR,
                new_x="LMARGIN",
                new_y="NEXT",
            )
    destination.parent.mkdir(parents=True, exist_ok=True)
    pdf.output(str(destination))
    if destination.stat().st_size < 100:
        raise RuntimeError("Source PDF is empty")
    print(f"Wrote {destination} ({len(pdf.pages)} pages)")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--output", type=Path, default=Path("_site/csharp/Platform.Data.Doublets.pdf")
    )
    parser.add_argument(
        "--font", type=Path, default=Path("/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf")
    )
    args = parser.parse_args()
    build_pdf(Path(__file__).resolve().parents[1], args.output, args.font)
