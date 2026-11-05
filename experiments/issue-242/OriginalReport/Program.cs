using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Memory;

using var links = new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory(), 4096);
var converter = new BalancedVariantConverter<ulong>(links);
var name_root = converter.Convert("super_name".ToList().Select(c => (ulong)c).ToList());
var notation = links.FormatStructure(name_root, _ => false, true, true);
Console.WriteLine(notation);
const string expected = "(9:(8:(6:(1:~115 ~117) (2:~112 ~101)) (7:(3:~114 ~95) (4:~110 ~97))) (5:~109 ~101))";
if (notation != expected)
    throw new InvalidOperationException($"Unexpected structure: {notation}");
