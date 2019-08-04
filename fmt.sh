#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

echo """
\\documentclass[7pt,a4paper,fleqn]{report}
\\usepackage[left=6mm,top=5mm,right=5mm,bottom=7mm,landscape]{geometry}
\\textwidth=283mm
\\pagestyle{plain}
\\usepackage[utf8]{inputenc}
\\usepackage[T1]{fontenc}
\\usepackage[T2A]{fontenc}
\\usepackage[gray]{xcolor}
\\usepackage{minted}

\\makeatletter
\\let\\xUTFviii@two@octets\\UTFviii@two@octets

\\def\\UTFviii@two@octets#1#2{%
\\ifx\\FancyVerbBreakAnywhereBreak#2%
\\expandafter\\xUTFviii@two@octets\\expandafter#1%
\\else
\\xUTFviii@two@octets#1#2%
\\fi
}
\\makeatother

\\usepackage{multicol}
\\usepackage{makeidx}
\\usepackage[columns=2]{idxlayout}
\\makeindex
\\renewcommand{\\thesection}{\\arabic{chapter}.\\arabic{section}}
\\setcounter{chapter}{1}
\\setcounter{section}{0}
\\usepackage[tiny]{titlesec}
\\titlespacing\\chapter{0mm}{0mm}{0mm}
\\titlespacing\\section{0mm}{0mm}{0mm}
\\DeclareUnicodeCharacter{221E}{\\ensuremath{\\infty}}
\\DeclareUnicodeCharacter{FFFD}{\\ensuremath{ }}
\\usepackage{fancyhdr}
\\pagestyle{fancy}
\\fancyhf{}
\\fancyfoot[C]{\\thepage}
\\renewcommand{\\headrulewidth}{0mm}
\\renewcommand{\\footrulewidth}{0mm}
\\renewcommand{\\baselinestretch}{0.7}
\\begin{document}

\\newminted{cpp}{
    breaklines,
    breakanywhere
}

\\sf
\\noindent{\\Large LinksPlatform's Platform.Data.Doublets Class Library}
\\begin{multicols}{2}
"""

# CSharp
#find * -type f -iname '*.cs' -exec sh -c 'enconv "{}"' \;
find * -type f -iname '*.cs' | sort -b | python fmt.py

echo """
\\end{multicols}
\\begin{section}{fmt.sh}
\\vspace{2mm}
\\inputminted[tabsize=2,breaklines,linenos=true]{bash}{fmt.sh}
\\end{section}
\\begin{section}{fmt.py}
\\vspace{2mm}
\\inputminted[tabsize=2,breaklines,linenos=true]{python}{fmt.py}
\\end{section}
\\printindex
\\end{document}
"""