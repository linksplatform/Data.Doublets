#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

# Remove auto-generated code files
find ./Platform.${TRAVIS_REPO_NAME}/obj -type f -iname "*.cs" -delete
find ./Platform.${TRAVIS_REPO_NAME}.Tests/obj -type f -iname "*.cs" -delete

# Download fvextra package
wget https://raw.githubusercontent.com/gpoore/fvextra/cc1c0c5f7b92023cfec67084e2a87bdac520414c/fvextra/fvextra.sty

printf """
\\\documentclass[11pt,a4paper,fleqn]{report}
\\\usepackage[left=5mm,top=5mm,right=5mm,bottom=5mm]{geometry}
\\\textwidth=200mm
\\\usepackage[utf8]{inputenc}
\\\usepackage[T1]{fontenc}
\\\usepackage[T2A]{fontenc}
\\\usepackage{fvextra}
\\\usepackage{minted}
\\\usemintedstyle{vs}
\\\usepackage{makeidx}
\\\usepackage[columns=1]{idxlayout}
\\\makeindex
\\\renewcommand{\\\thesection}{\\\arabic{chapter}.\\\arabic{section}}
\\\setcounter{chapter}{1}
\\\setcounter{section}{0}
\\\usepackage[tiny]{titlesec}
\\\titlespacing\\\chapter{0mm}{0mm}{0mm}
\\\titlespacing\\\section{0mm}{0mm}{0mm}
\\\DeclareUnicodeCharacter{221E}{\\\ensuremath{\\\infty}}
\\\DeclareUnicodeCharacter{FFFD}{\\\ensuremath{ }}
\\\usepackage{fancyhdr}
\\\pagestyle{fancy}
\\\fancyhf{}
\\\fancyfoot[C]{\\\thepage}
\\\renewcommand{\\\headrulewidth}{0mm}
\\\renewcommand{\\\footrulewidth}{0mm}
\\\renewcommand{\\\baselinestretch}{0.7}
\\\begin{document}
\\\sf
\\\noindent{\\\Large LinksPlatform's Platform.${TRAVIS_REPO_NAME} Class Library}
"""

# Project files
find ./Platform.${TRAVIS_REPO_NAME} -type f -iname '*.cs' | sort -b | python format-csharp-files.py

# Tests files
find ./Platform.${TRAVIS_REPO_NAME}.Tests -type f -iname '*.cs' | sort -b | python format-csharp-files.py

printf """
\\\printindex
\\\end{document}
"""
