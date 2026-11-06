## csharp 1.0.0

- Require .NET 10, update dependency APIs and native tools, and repair abstract native method interfaces for current compilers.

## cpp 1.0.0

- Require .NET 10, update dependency APIs and native tools, and repair abstract native method interfaces for current compilers.

## csharp 0.18.10

- Format raw sequence leaves safely, preserve nested debug markers, and add equivalent C++ structure formatting.

## cpp 0.2.0

- Format raw sequence leaves safely, preserve nested debug markers, and add equivalent C++ structure formatting.

## csharp 0.18.9

- Match source and target restrictions in their own positions for concrete-index Count and Each queries in united and split stores.

## cpp 0.1.6

- Match source and target restrictions in their own positions for concrete-index Count and Each queries in united and split stores.

## csharp 0.18.8

- DeleteAll deletes every link, including links above Count() after deletions, and resets endpoints before each deletion so the store can be reused; native GetOrCreate and CreateAndUpdate return the created link address

## cpp 0.1.5

- DeleteAll deletes every link, including links above Count() after deletions, and resets endpoints before each deletion so the store can be reused; native GetOrCreate and CreateAndUpdate return the created link address

## csharp 0.18.7

- MergeUsages and MergeAndDelete rewrite only the merged endpoint of each usage; the native handler-taking Delete no longer recurses

## cpp 0.1.4

- MergeUsages and MergeAndDelete rewrite only the merged endpoint of each usage; the native handler-taking Delete no longer recurses

## csharp 0.18.6

- Count and enumerate each link once for either-endpoint queries, including equal source and target values

## cpp 0.1.3

- Count and enumerate each link once for either-endpoint queries, including equal source and target values

## csharp 0.18.5

- Return surviving link addresses after uniqueness resolution, preserve merged usage endpoints and retain creation callback Break state.

## cpp 0.1.2

- Include NuGet's required license compatibility URL in native packages and validate embedded license metadata before publication.

## csharp 0.18.4

- Document caller-selected update validation, raw update preconditions and checked update recipes; verify missing updates preserve store state.
- Keep memory-store deletion unchecked and provide optional missing-link deletion policies with C# and Rust design guidance.
- Restore warning-free CI, nullable contracts, executable native tests, reproducible packages, documentation and guarded publishing.
- Publish C# and C++ packages using GitHub OIDC and fresh NuGet trusted publishing credentials, with actionable pre-build policy and package-scope checks.

## cpp 0.1.1

- Restore warning-free CI, nullable contracts, executable native tests, reproducible packages, documentation and guarded publishing.
- Publish C# and C++ packages using GitHub OIDC and fresh NuGet trusted publishing credentials, with actionable pre-build policy and package-scope checks.

