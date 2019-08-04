FILE=fmt
${FILE}.dvi: ${FILE}.tex
	latex -shell-escape ${FILE}.tex
	makeindex ${FILE}
	latex -shell-escape ${FILE}.tex
	dvipdf ${FILE}.dvi ${FILE}.pdf
	dvips ${FILE}.dvi