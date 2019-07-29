dotnet pack -c Release
cd bin\Release\
nuget push -Source https://api.nuget.org/v3/index.json *.nupkg
del *.nupkg
del *.snupkg
cd ..\..