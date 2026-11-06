BenchmarkDotNet 0.15.8 pins Perfolizer exactly `[0.6.1]`. The current Perfolizer stable release is 0.7.5 (Pragmastat 14.0.1), so transitive upgrades cannot advance this branch without violating the parent's exact package constraint.

Reproduce with SDK 10.0.403 and Central Package Management:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="BenchmarkDotNet" Version="0.15.8" />
    <PackageVersion Include="Perfolizer" Version="0.7.5" />
  </ItemGroup>
</Project>
```

Reference BenchmarkDotNet in a net10.0 executable and run `dotnet restore --force-evaluate`, followed by `dotnet restore --locked-mode`. In Data.Doublets the latter fails NU1102 (`Perfolizer >= 0.7.5`, nearest version `0.7.5`), while the lock records the `[0.6.1]` parent requirement. Logs: https://github.com/linksplatform/Data.Doublets/pull/557 (dev/log/issues/556/pulls/557/validation/csharp-full-refresh.log).

Local workaround: retain Perfolizer 0.6.1, keeping the upstream exact constraint honest. Other transitive dependencies are upgraded explicitly and exercised by the benchmark smoke test.

Feature proposal: adapt the statistical calls to current Perfolizer/Pragmastat APIs in the next BenchmarkDotNet release and update its exact Perfolizer reference. If the exact pin is necessary for behavioral compatibility, please document that constraint and the migration path for consumers maintaining current dependency trees. This report does not request weakening the constraint without testing benchmark statistics.
