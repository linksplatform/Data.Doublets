using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

using var links = new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory(), 4096);
// Build the same adjacent-pair layers as BalancedVariantConverter for issue #242.
var layer = "super_name".Select(c => (ulong)c).ToArray();
while (layer.Length > 1)
{
    var next = new List<ulong>();
    for (var i = 0; i < layer.Length; i += 2)
        next.Add(i + 1 < layer.Length ? links.GetOrCreate(layer[i], layer[i + 1]) : layer[i]);
    layer = next.ToArray();
}
var notation = links.FormatStructure(layer[0], _ => false, true, true);
Console.WriteLine(notation);
const string expected = "(9:(8:(6:(1:~115 ~117) (2:~112 ~101)) (7:(3:~114 ~95) (4:~110 ~97))) (5:~109 ~101))";
if (notation != expected)
    throw new InvalidOperationException($"Unexpected structure: {notation}");
