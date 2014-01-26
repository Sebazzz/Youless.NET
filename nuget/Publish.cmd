@echo off
NuGet Pack YoulessNet.nuspec
Copy /B *.nupkg YoulessNet.nuspec.nupkg
NuGet Push YoulessNet.nuspec.nupkg
Erase *.nupkg
Pause