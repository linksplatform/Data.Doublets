dotnet pack -c Release
dotnet nuget push -s https://api.nuget.org/v3/index.json **/*.nupkg
del /s /q *.nupkg
del /s /q *.snupkg