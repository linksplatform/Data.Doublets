from pathlib import Path
import re
root=Path('csharp/Platform.Data.Doublets')
for p in root.rglob('*.cs'):
 s=p.read_text()
 s=s.replace('handlerState.Handle(', 'handlerState.HandleNullable(')
 # Require values only at operations that dereference them; optional formatting
 # and create/delete transition snapshots retain null semantics.
 if p.name in ('ILinksExtensions.cs','UInt64Links.cs','UniLinks.cs','UInt64LinksTransactionsLayer.cs'):
  s=s.replace('.GetLink(', '.GetRequiredLink(')
 if p.name=='ILinksExtensions.cs':
  s=s.replace('return link[links.Constants.', 'return (link ?? throw new ArgumentNullException(nameof(link)))[links.Constants.')
  s=s.replace('firstLink = link[','firstLink = (link ?? throw new InvalidOperationException("Read callback did not provide a link."))[')
  s=s.replace('filler.AddFirstAndReturnConstant, restriction','link => filler.AddFirstAndReturnConstant(link ?? throw new InvalidOperationException("Read callback did not provide a link.")), restriction')
  s=s.replace('queryResultFiller.AddFirstAndReturnConstant, query','link => queryResultFiller.AddFirstAndReturnConstant(link ?? throw new InvalidOperationException("Read callback did not provide a link.")), query')
  for name in ('EnsureLinkExists','EnsureInnerReferenceExists','EnsureLinkIsAnyOrExists','Update','DeleteMany'):
   s=re.sub(r'(public static [^\n]+ '+name+r'<TLinkAddress>\([^\n]+(?:restriction|deletedLinks)[^\n]*\n\s*\{)', lambda m:m[0]+'\n            '+('deletedLinks' if name=='DeleteMany' else 'restriction')+' ??= Array.Empty<TLinkAddress>();',s)
  s=s.replace('Singles the or TLinkAddress.Zero','Singles the or default')
 if p.name=='UInt64Links.cs':
  marker='''public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {'''
  s=s.replace(marker,marker+'\n            System.ArgumentNullException.ThrowIfNull(restriction);\n            System.ArgumentNullException.ThrowIfNull(substitution);')
 if p.name=='LinksItselfConstantToSelfReferenceResolver.cs':
  s=s.replace('restriction.Contains(item: itselfConstant)', 'restriction != null && restriction.Contains(item: itselfConstant)')
 if p.name=='UInt64LinksTransactionsLayer.cs':
  s=s.replace('private Queue<Transition> _currentTransactionTransitions;', 'private Queue<Transition>? _currentTransactionTransitions;').replace('private Transaction _currentTransaction;', 'private Transaction? _currentTransaction;')
  s=s.replace('IList<TLinkAddress> before, IList<TLinkAddress> after)', 'IList<TLinkAddress>? before, IList<TLinkAddress>? after)')
 if p.name=='UniLinks.cs':
  s=s.replace('patternOrCondition.IsNullOrEmpty()', '(patternOrCondition is not { Count: > 0 })').replace('substitution.IsNullOrEmpty()', '(substitution is not { Count: > 0 })')
  s=s.replace('patternOrCondition.EqualTo(right: substitution)', 'patternOrCondition != null && substitution != null && patternOrCondition.EqualTo(right: substitution)')
  s=s.replace('(IList<TLinkAddress>?)substitution.ToArray()', '(IList<TLinkAddress>)substitution.ToArray()')
  s=s.replace('return matchHandler != null ? substitutionHandler(', 'return substitutionHandler != null ? substitutionHandler(')
  # The update path requires both a condition and substitution.
  s=s.replace('// Replace / Update\n', '// Replace / Update\n        ArgumentNullException.ThrowIfNull(patternOrCondition);\n        ArgumentNullException.ThrowIfNull(substitution);\n')
 if p.name=='ILinksExtensions.cs' and p.parent.name=='Tests':
  s=s.replace('var i = default(T)', 'var i = T.Zero').replace('T i = default', 'T i = T.Zero')
 p.write_text(s)
p=root/'NullableLinkHelpers.cs'
p.write_text('''using System;
using System.Collections.Generic;
using System.Numerics;

namespace Platform.Data.Doublets;

// Platform.Data 0.16.1 annotates these callback snapshots as non-null even
// though creation/deletion legitimately supply null. Keep their semantics
// until the upstream annotations are corrected.
internal static class NullableLinkHelpers
{
    internal static T HandleNullable<T>(this ref WriteHandlerState<T> state, IList<T>? before, IList<T>? after) where T : IUnsignedNumber<T>
    {
        if (state.Handler is { } handler)
        {
            state.Apply(handler(before, after));
        }
        return state.Result;
    }

    internal static IList<T> GetRequiredLink<T>(this ILinks<T> links, T index) where T : IUnsignedNumber<T>
        => links.GetLink(index) ?? throw new KeyNotFoundException($"Link {index} does not exist.");
}
''')
# Package metadata: retain identity, embed an icon, modernize SourceLink.
p=root/'Platform.Data.Doublets.csproj';s=p.read_text(encoding='utf-8-sig').replace('<VersionPrefix>0.18.2</VersionPrefix>','<VersionPrefix>0.18.3</VersionPrefix>')
s=re.sub(r'<PackageIconUrl>.*?</PackageIconUrl>','<PackageIcon>icon.png</PackageIcon>',s)
s=s.replace('git://github.com/', 'https://github.com/').replace('Version="1.1.1"', 'Version="8.0.0"').replace('Fix split-memory exact lookups and counts for internal sources with external targets.','Validate all CI outputs, modernize documentation and packaging, and correct nullable link API contracts.')
s=s.replace('<None Include="..\\..\\README.md"', '<None Include="../../assets/icon.png" Pack="true" PackagePath="/"/>\n        <None Include="..\\..\\README.md"')
p.write_text(s)
p=Path('cpp/Platform.Data.Doublets/Platform.Data.Doublets.TemplateLibrary.nuspec');s=p.read_text().replace('<version>0.1.0</version>','<version>0.1.1</version>').replace('Translated build to conan 2.0','Execute CTest in CI, validate native packages, and use reproducible Conan 2 tooling.');p.write_text(s)
