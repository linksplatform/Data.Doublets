#!/usr/bin/python
# -*- coding: utf-8 -*-
import sys
reload(sys)
sys.setdefaultencoding('utf-8')
for line in sys.stdin.readlines():
    line = line.strip()
    print "\\index{%s}" % (line.replace('_','\\_'))
    print "\\begin{section}{%s}" % (line.replace('_','\\_'))
    #print "\\inputminted[tabsize=2,breaklines,linenos=true]{csharp}{%s}" % (line)
    print "\\begin{minted}[tabsize=2,breaklines,breakanywhere,linenos=true,xleftmargin=7mm,framesep=4mm]{csharp}"
    f = open(line,"rt")
    c = "\n".join([x.strip("\n") for x in f.readlines()])
    f.close()
    c = c.replace(u'\ufeff','')
    print c
    print "\\end{minted}"
    print "\\end{section}"
    print
