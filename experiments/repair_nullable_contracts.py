"""One-time mechanical repairs; behavioral changes have regression coverage."""
from pathlib import Path
import re
root=Path(__file__).resolve().parents[1]/'csharp/Platform.Data.Doublets'
for p in root.rglob('*.cs'):
 s=p.read_text(encoding='utf-8-sig')
 if 'TLinkAddress : IUnsignedNumber<TLinkAddress>' in s:
  s=re.sub(r'\bdefault\(TLinkAddress\)', 'TLinkAddress.Zero',s)
  s=re.sub(r'\bdefault\b(?!\s*:)', 'TLinkAddress.Zero',s) if p.name in ('Link.cs','ILinksExtensions.cs','PropertyOperator.cs','PropertiesOperator.cs','UnitedMemoryLinksBase.cs','SplitMemoryLinksBase.cs') and p.parent.name!='Tests' else s
 s=s.replace('Equals(object obj)', 'Equals(object? obj)').replace('Equals(object other)', 'Equals(object? other)')
 if p.name in ('UnitedMemoryLinksBase.cs','SplitMemoryLinksBase.cs'):
  s=s.replace('public IList<TLinkAddress>? GetLinkStruct', 'public IList<TLinkAddress> GetLinkStruct')
  s=s.replace('public virtual TLinkAddress Count(IList<TLinkAddress>? restriction)\n        {','public virtual TLinkAddress Count(IList<TLinkAddress>? restriction)\n        {\n            restriction ??= System.Array.Empty<TLinkAddress>();')
  s=s.replace('public virtual TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)\n        {','public virtual TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)\n        {\n            restriction ??= System.Array.Empty<TLinkAddress>();\n            handler ??= _ => Constants.Continue;')
  s=s.replace('public virtual TLinkAddress Count(IList<TLinkAddress>? restriction)\n    {','public virtual TLinkAddress Count(IList<TLinkAddress>? restriction)\n    {\n        restriction ??= System.Array.Empty<TLinkAddress>();')
  s=s.replace('public virtual TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)\n    {','public virtual TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)\n    {\n        restriction ??= System.Array.Empty<TLinkAddress>();\n        handler ??= _ => Constants.Continue;')
  # Index helpers are initialized by derived stores and invalidated on disposal.
  # Preserve protected member names using checked backing properties.
  pat=r'protected (ILinks(?:Tree|List)Methods<TLinkAddress>) (\w+);'
  def field(m):
   typ,name=m.groups(); back='_'+name[0].lower()+name[1:]
   return f'private {typ}? {back};\n        protected {typ} {name}\n        {{\n            get => {back} ?? throw new System.ObjectDisposedException(GetType().Name, "Store indices are not initialized or have been disposed.");\n            set => {back} = value;\n        }}'
  names=re.findall(pat,s)
  s=re.sub(pat,field,s)
  for typ,name in names:
   s=s.replace(name+' = null;', '_'+name[0].lower()+name[1:]+' = null;')
 if p.name=='LinksDecoratorBase.cs': s=s.replace('Facade = this;', '_facade = this;\n        Facade = this;')
 if p.name=='ILinksExtensions.cs' and p.parent.name=='Tests':
  s=s.replace('string message)', 'string? message)').replace('string message,','string? message,').replace('string messageBuilder()', 'string? messageBuilder()').replace('var i = default(T);','var i = T.Zero;')
 # Every tree accepts an optional read handler, including direct helper calls.
 s=re.sub(r'handler\(link: (GetLinkValues\(linkIndex: link\))\)',r'(handler?.Invoke(link: \1) ?? Continue)',s)
 p.write_text(s)
