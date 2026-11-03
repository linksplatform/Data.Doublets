"""One-time, compiler-directed repair of malformed existing XML documentation."""
from pathlib import Path
import re, sys, collections
root=Path(__file__).resolve().parents[1]
warnings=collections.defaultdict(set)
for line in Path(sys.argv[1]).read_text().splitlines():
 m=re.search(r'/csharp/(.*?)\((\d+),\d+\): warning (CS\d+): (.*) \[',line)
 if m: warnings[root/'csharp'/m[1]].add((int(m[2])-1,m[3],m[4]))
for path, entries in warnings.items():
 s=path.read_text(encoding='utf-8-sig'); lines=s.splitlines(True)
 for row,code,msg in sorted(entries,reverse=True):
  if code=='CS1574':
   name=re.search("attribute '([^']+)'",msg)[1]
   matches=re.findall(r'(?:class|struct|interface) '+re.escape(name)+r'<([^>]+)>',s)
   if matches: lines[row]=lines[row].replace('cref="'+name+'"','cref="'+name+'{'+matches[0]+'}"')
  elif code=='CS1572':
   # XML names never include C#'s @ escape prefix.
   name=re.search("tag for '([^']+)'",msg)[1]
   if name.startswith('@'): lines[row]=lines[row].replace('name="'+name+'"','name="'+name[1:]+'"')
   elif name=='restriction': lines[row]=lines[row].replace('restriction','substitution')
  elif code=='CS1573':
   name=re.search("Parameter '([^']+)'",msg)[1]
   start=row
   while start>0 and ('///' in lines[start-1] or lines[start-1].lstrip().startswith('[')): start-=1
   indent=re.match(r'\s*',lines[row])[0]
   descriptions={'handler':'Optional callback; null continues without invoking a callback.','substitution':'Values to create or substitute.','restriction':'Values used to select matching links.','constants':'Constants defining the link address domain.','indexTreeType':'Tree implementation used to index links.'}
   lines.insert(start,indent+'/// <param name="'+name+'">'+descriptions.get(name,'The '+re.sub(r'([A-Z])',r' \1',name).lower()+'.')+'</param>\n')
  elif code=='CS1711':
   start=row
   while '</typeparam>' not in lines[row]: row+=1
   del lines[start:row+1]
  elif code=='CS1591':
   indent=re.match(r'\s*',lines[row])[0]
   text='Logs link mutations and forwards optional callbacks.' if 'class LoggingDecorator' in lines[row] else 'Returns the break constant when an operation throws.' if 'class NoExceptionsDecorator' in lines[row] else 'Initializes the decorator with its underlying links.' if re.search(r'public (LoggingDecorator|NoExceptionsDecorator)\(',lines[row]) else None
   lines.insert(row,indent+('/// <summary>'+text+'</summary>\n' if text else '/// <inheritdoc/>\n'))
 for i,line in enumerate(lines):
  line=line.replace('cref="ILinks{TLinkAddress, LinksConstants{TLinkAddress}}"','cref="Platform.Data.ILinks{TLinkAddress, TConstants}"')
  line=line.replace('cref="ISynchronizedLinks{TLinkAddress, LinksConstants{TLinkAddress}}"','cref="Platform.Data.ISynchronizedLinks{TLinkAddress, TConstants}"')
  lines[i]=line
 path.write_text(''.join(lines))
