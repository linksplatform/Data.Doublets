[![Build Status](https://travis-ci.com/linksplatform/Data.Doublets.svg?branch=master)](https://travis-ci.com/linksplatform/Data.Doublets)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/83c66adb68f44a018c795bc7dc7d6f49)](https://app.codacy.com/app/drakonard/Data.Doublets?utm_source=github.com&utm_medium=referral&utm_content=linksplatform/Data.Doublets&utm_campaign=Badge_Grade_Dashboard)
[![CodeFactor](https://www.codefactor.io/repository/github/linksplatform/data.doublets/badge/master)](https://www.codefactor.io/repository/github/linksplatform/data.doublets/overview/master)

[![Поддержать разработку](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.herokuapp.com%2Fkonard%2Fpledgesssss&style=for-the-badge "Поддержать разработку")](https://patreon.com/konard)

# [Data.Doublets](https://github.com/linksplatform/Data.Doublets) ([english version](README.md))

Библиотека классов ПлатформыСвязей Platform.Data.Doublets.

Пространство имён: [Platform.Data.Doublets](https://linksplatform.github.io/Data.Doublets/api/Platform.Data.Doublets.html)

Ответвление от: [Konard/LinksPlatform/Platform/Platform.Data.Doublets](https://github.com/Konard/LinksPlatform/tree/b0844d778ced60b22435e57342393031b26a2822/Platform/Platform.Data.Doublets)

NuGet пакет: [Platform.Data.Doublets](https://www.nuget.org/packages/Platform.Data.Doublets)

## [Пример](https://github.com/linksplatform/HelloWorld.Doublets.DotNet)

```C#
using System;
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
      }
    }
  }
}
```

## [SQLite против Дуплетов](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets)

## [Документация](https://linksplatform.github.io/Data.Doublets)
* Интерфейс [ILinks\<TLink, TConstants\>](https://linksplatform.github.io/Data/api/Platform.Data.ILinks-2.html).
* Интерфейс [ILinks\<TLink\>](https://linksplatform.github.io/Data.Doublets/api/Platform.Data.Doublets.ILinks-1.html).
* Класс [ResizableDirectMemoryLinks\<TLink\>](https://linksplatform.github.io/Data.Doublets/api/Platform.Data.Doublets.ResizableDirectMemory.ResizableDirectMemoryLinks-1.html).

[PDF файл](https://linksplatform.github.io/Data.Doublets/Platform.Data.Doublets.pdf) с кодом для электронных книг.

## Зависит от
* [Platform.Random](https://github.com/linksplatform/Random)
* [Platform.Timestamps](https://github.com/linksplatform/Timestamps)
* [Platform.Collections.Methods](https://github.com/linksplatform/Collections.Methods)
* [Platform.Singletons](https://github.com/linksplatform/Singletons)
* [Platform.Memory](https://github.com/linksplatform/Memory)
* [Platform.Data](https://github.com/linksplatform/Data)

## Загадочные файлы
* [.travis.yml](https://github.com/linksplatform/Data.Doublets/blob/master/.travis.yml) - конфигурация сборки Travis CI.
* [docfx.json](https://github.com/linksplatform/Data.Doublets/blob/master/docfx.json) и [toc.yml](https://github.com/linksplatform/Data.Doublets/blob/master/toc.yml) - конфигурация сборки DocFX.
* [format-document.sh](https://github.com/linksplatform/Data.Doublets/blob/master/format-document.sh) - скрипт для форматирования `tex` файла для генерации PDF из него.
* [format-csharp-files.py](https://github.com/linksplatform/Data.Doublets/blob/master/format-csharp-files.py) - скрипт для форматирования одного файла `.cs` как части файла `tex`.
* [generate-pdf.sh](https://github.com/linksplatform/Data.Doublets/blob/master/generate-pdf.sh) - скрипт, который генерирует PDF с кодом для электронных книг.
* [publish-docs.sh](https://github.com/linksplatform/Data.Doublets/blob/master/publish-docs.sh) - скрипт, который публикует сгенерированную документацию и PDF с кодом для электронных книг в ветку `gh-pages`.
* [push-nuget.bat](https://github.com/linksplatform/Data.Doublets/blob/master/push-nuget.bat) - Windows-скрипт для публикации текущей версии пакета NuGet.
