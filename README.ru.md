[![Версия NuGet пакета и количество загрузок](https://buildstats.info/nuget/Platform.Data.Doublets)](https://www.nuget.org/packages/Platform.Data.Doublets)
[![Состояние сборки](https://travis-ci.com/linksplatform/Data.Doublets.svg?branch=master)](https://travis-ci.com/linksplatform/Data.Doublets)
[![Codacy](https://api.codacy.com/project/badge/Grade/83c66adb68f44a018c795bc7dc7d6f49)](https://app.codacy.com/app/drakonard/Data.Doublets?utm_source=github.com&utm_medium=referral&utm_content=linksplatform/Data.Doublets&utm_campaign=Badge_Grade_Dashboard)
[![CodeFactor](https://www.codefactor.io/repository/github/linksplatform/data.doublets/badge/master)](https://www.codefactor.io/repository/github/linksplatform/data.doublets/overview/master)

# [Data.Doublets](https://github.com/linksplatform/Data.Doublets) ([english version](README.md))
Библиотека классов ПлатформыСвязей Platform.Data.Doublets.

Пространство имён: [Platform.Data.Doublets](https://linksplatform.github.io/Data.Doublets/api/Platform.Data.Doublets.html)

Ответвление от: [Konard/LinksPlatform/Platform/Platform.Data.Doublets](https://github.com/Konard/LinksPlatform/tree/b0844d778ced60b22435e57342393031b26a2822/Platform/Platform.Data.Doublets)

NuGet пакет: [Platform.Data.Doublets](https://www.nuget.org/packages/Platform.Data.Doublets)

## [Пример](https://github.com/linksplatform/HelloWorld.Doublets.DotNet)
```C#
using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.ResizableDirectMemory;

namespace HelloWorld.Doublets.DotNet
{
  class Program
  {
    static void Main()
    {
      using (var links = new ResizableDirectMemoryLinks<uint>("db.links"))
      {
        var link = links.Create();
        link = links.Update(link, link, link);
        Console.WriteLine("Привет Мир!");
        Console.WriteLine($"Это моя первая связь: ({link}:{links.GetSource(link)}->{links.GetTarget(link)}).");
        Console.WriteLine($"Всего связей в хранилище: {links.Count()}.");
        link = links.Update(link, default, default);
        links.Delete(link);
      }
    }
  }
}
```

## [SQLite против Дуплетов](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)

## [Документация](https://linksplatform.github.io/Data.Doublets)
*   Интерфейс [ILinks\<TLink, TConstants\>](https://linksplatform.github.io/Data/api/Platform.Data.ILinks-2.html).
*   Интерфейс [ILinks\<TLink\>](https://linksplatform.github.io/Data.Doublets/api/Platform.Data.Doublets.ILinks-1.html).
*   Класс [ResizableDirectMemoryLinks\<TLink\>](https://linksplatform.github.io/Data.Doublets/api/Platform.Data.Doublets.ResizableDirectMemory.ResizableDirectMemoryLinks-1.html).

[PDF файл](https://linksplatform.github.io/Data.Doublets/Platform.Data.Doublets.pdf) с кодом для электронных книг.

## Граф зависимостей
[![SVG изображение графа зависимостей](https://raw.github.com/linksplatform/Documentation/master/doc/Dependencies/Platform.Data.Doublets.svg?sanitize=true)](https://raw.githubusercontent.com/linksplatform/Documentation/master/doc/Dependencies/Platform.Data.Doublets.svg?sanitize=true)

## Зависит напрямую от
*   [Platform.Random](https://github.com/linksplatform/Random)
*   [Platform.Timestamps](https://github.com/linksplatform/Timestamps)
*   [Platform.Collections.Methods](https://github.com/linksplatform/Collections.Methods)
*   [Platform.Singletons](https://github.com/linksplatform/Singletons)
*   [Platform.Memory](https://github.com/linksplatform/Memory)
*   [Platform.Data](https://github.com/linksplatform/Data)
