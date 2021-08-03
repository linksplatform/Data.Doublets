[![NuGet Version and Downloads count](https://buildstats.info/nuget/Platform.Data.Doublets)](https://www.nuget.org/packages/Platform.Data.Doublets)
[![Actions Status](https://github.com/linksplatform/Data.Doublets/workflows/CD/badge.svg)](https://github.com/linksplatform/Data.Doublets/actions?workflow=CD)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/d92f59d08c604e95ba2469ee8e9d88c1)](https://app.codacy.com/gh/linksplatform/Data.Doublets?utm_source=github.com&utm_medium=referral&utm_content=linksplatform/Data.Doublets&utm_campaign=Badge_Grade_Settings)
[![CodeFactor](https://www.codefactor.io/repository/github/linksplatform/data.doublets/badge/master)](https://www.codefactor.io/repository/github/linksplatform/data.doublets/overview/master)

# [Data.Doublets](https://github.com/linksplatform/Data.Doublets) ([русская версия](README.ru.md))
LinksPlatform's Platform.Data.Doublets Class Library.

Namespace: [Platform.Data.Doublets](https://linksplatform.github.io/Data.Doublets/csharp/api/Platform.Data.Doublets.html)

Forked from: [Konard/LinksPlatform/Platform/Platform.Data.Doublets](https://github.com/Konard/LinksPlatform/tree/b0844d778ced60b22435e57342393031b26a2822/Platform/Platform.Data.Doublets)

NuGet package: [Platform.Data.Doublets](https://www.nuget.org/packages/Platform.Data.Doublets)

## [Example](https://github.com/linksplatform/Examples.Doublets.CRUD.DotNet)
```C#
using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

// A doublet links store is mapped to "db.links" file:
using var links = new UnitedMemoryLinks<uint>("db.links");

// A creation of the doublet link: 
var link = links.Create();

// The link is updated to reference itself twice (as a source and a target):
link = links.Update(link, newSource: link, newTarget: link);

// Read operations:
Console.WriteLine($"The number of links in the data store is {links.Count()}.");
Console.WriteLine("Data store contents:");
var any = links.Constants.Any; // Means any link address or no restriction on link address
// Arguments of the query are interpreted as restrictions
var query = new Link<uint>(index: any, source: any, target: any);
links.Each((link) => {
    Console.WriteLine(links.Format(link));
    return links.Constants.Continue;
}, query);

// The link's content reset:
link = links.Update(link, newSource: default, newTarget: default);

// The link deletion:
links.Delete(link);
```

## [SQLite vs Doublets](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)

[![Image with result of performance comparison between SQLite and Doublets.](https://raw.githubusercontent.com/linksplatform/Documentation/master/doc/Examples/sqlite_vs_doublets_performance.png "Result of performance comparison between SQLite and Doublets")](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)

## [Documentation](https://linksplatform.github.io/Data.Doublets)
*   Interface [ILinks\<TLink, TConstants\>](https://linksplatform.github.io/Data/csharp/api/Platform.Data.ILinks-2.html).
*   Interface [ILinks\<TLink\>](https://linksplatform.github.io/Data.Doublets/csharp/api/Platform.Data.Doublets.ILinks-1.html).
*   Class [UnitedMemoryLinks\<TLink\>](https://linksplatform.github.io/Data.Doublets/csharp/api/Platform.Data.Doublets.Memory.United.Generic.UnitedMemoryLinks-1.html).

[PDF file](https://linksplatform.github.io/Data.Doublets/csharp/Platform.Data.Doublets.pdf) with code for e-readers.

## Dependency graph [C#]
[![C# dependency graph SVG image](https://raw.github.com/linksplatform/Documentation/master/doc/Dependencies/Platform.Data.Doublets.svg?sanitize=true)](https://raw.githubusercontent.com/linksplatform/Documentation/master/doc/Dependencies/Platform.Data.Doublets.svg?sanitize=true)

## Dependency graph [C++]
[![C++ dependency graph SVG image](https://raw.github.com/linksplatform/Documentation/master/doc/Dependencies/Platform.Data.Doublets.cpp.svg?sanitize=true)](https://raw.githubusercontent.com/linksplatform/Documentation/master/doc/Dependencies/Platform.Data.Doublets.cpp.svg?sanitize=true)

## Depend on
*   [Platform.Random](https://github.com/linksplatform/Random)
*   [Platform.Timestamps](https://github.com/linksplatform/Timestamps)
*   [Platform.Incrementers](https://github.com/linksplatform/Incrementers)
*   [Platform.Collections.Methods](https://github.com/linksplatform/Collections.Methods)
*   [Platform.Singletons](https://github.com/linksplatform/Singletons)
*   [Platform.Memory](https://github.com/linksplatform/Memory)
*   [Platform.Data](https://github.com/linksplatform/Data)

## Support

Ask questions at [stackoverflow.com/tags/links-platform](https://stackoverflow.com/tags/links-platform) (or with tag `links-platform`) to get our free support.

You can also get real-time support on [our official Discord server](https://discord.gg/eEXJyjWv5e).
