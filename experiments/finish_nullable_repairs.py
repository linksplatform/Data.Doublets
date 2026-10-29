from pathlib import Path
import re
root=Path('csharp/Platform.Data.Doublets')
for p in root.rglob('*.cs'):
 s=p.read_text()
 s=re.sub(r'\(handler\?\.Invoke\(link: (GetLinkValues\(linkIndex: (?:link|current)\))\) \?\? Continue\)',r'(handler == null ? Continue : handler(link: \1))',s)
 s=re.sub(r'handler\(link: (GetLinkValues\(linkIndex: current\))\)',r'(handler == null ? Continue : handler(link: \1))',s)
 if p.name in ('SplitMemoryLinksBase.cs','UnitedMemoryLinksBase.cs'):
  s=s.replace('TLinkAddress _zero;', 'TLinkAddress _zero = TLinkAddress.Zero;').replace('TLinkAddress _one = ++_zero;', 'TLinkAddress _one = TLinkAddress.One;')
  s=re.sub(r'(public virtual TLinkAddress Delete\(IList<TLinkAddress>\? restriction, WriteHandler<TLinkAddress>\? handler\)\n\s*\{)',r'\1\n            System.ArgumentNullException.ThrowIfNull(restriction);',s)
  s=s.replace('protected InternalLinksSourcesLinkedListMethods<TLinkAddress> InternalSourcesListMethods;', '''private InternalLinksSourcesLinkedListMethods<TLinkAddress>? _internalSourcesListMethods;
    protected InternalLinksSourcesLinkedListMethods<TLinkAddress> InternalSourcesListMethods
    {
        get => _internalSourcesListMethods ?? throw new System.ObjectDisposedException(GetType().Name);
        set => _internalSourcesListMethods = value;
    }''').replace('InternalSourcesListMethods = null;', '_internalSourcesListMethods = null;')
 if p.name=='NullableLinkHelpers.cs':
  s=s.replace('T index)', 'T link)').replace('links.GetLink(index)', 'links.GetLink(link)').replace('{index}', '{link}')
 if p.name=='ILinksExtensions.cs' and p.parent.name!='Tests': s=s.replace('nonExistentAddresses.Max();','nonExistentAddresses.Aggregate((left, right) => Comparer<TLinkAddress>.Default.Compare(left, right) >= 0 ? left : right);')
 if p.name=='ILinksExtensions.cs' and p.parent.name=='Tests':s=s.replace('var zero = default(T);','var zero = T.Zero;').replace('var one = ++zero;','var one = T.One;')
 if p.name=='PropertyOperator.cs': s=s.replace('? default :','? TLinkAddress.Zero :')
 if p.name=='ISynchronizedLinks.cs': s=s.replace('ISynchronizedLinks{TLinkAddress, ILinks{TLinkAddress}, LinksConstants{TLinkAddress}}','Platform.Data.ISynchronizedLinks{TLinkAddress, TLinks, TConstants}')
 if p.name=='UInt64LinksTransactionsLayer.cs':s=s.replace('private readonly Task _transitionsPusher;', 'private Task? _transitionsPusher;').replace('private Task _transitionsPusher;', 'private Task? _transitionsPusher;')
 p.write_text(s)
