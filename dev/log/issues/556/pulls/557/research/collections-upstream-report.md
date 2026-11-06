NuGet's flat-container index currently lists `Platform.Collections.Methods` 1.0.0 as the highest stable version. Updating from 0.7.3 removes the `Platform.Collections.Methods.Trees` and `.Lists` namespaces used by Data.Doublets.

Minimal reproduction (SDK 10.0.403):

```sh
dotnet new classlib -n Repro -f net10.0
cd Repro
dotnet add package Platform.Collections.Methods --version 1.0.0
printf 'using Platform.Collections.Methods.Trees;\npublic class Repro {}\n' > Class1.cs
dotnet build
```

Expected: the tree methods remain available or a migration guide identifies their replacements.
Actual: CS0234, namespace `Methods` does not exist. The 0.7.3 nupkg includes net7.0 binaries and XML documenting 127 `Platform.Collections.Methods` members; 1.0.0 contains netstandard2.0 binaries without those documented APIs. I could not find a matching 1.0.0 source tag or migration guide.

Local workaround: keep 0.7.3 in https://github.com/linksplatform/Data.Doublets/pull/557; upgrading the full solution produces 176 compile errors, including missing `SizedAndThreadedAVLBalancedTreeMethods` and `RecursionlessSizeBalancedTreeMethods`.

Suggested resolution: publish a corrected higher stable version from current source preserving the `Platform.Collections.Methods.Trees` and `.Lists` API, or document the intended new namespaces/types. If 1.0.0 is an obsolete/mistaken publication, unlist it and publish the current API under a higher version so automated updaters resolve the intended package.
