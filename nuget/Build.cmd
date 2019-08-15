REM TODO: Add check that nuget.exe is installed.

@echo off
echo Enter semver version number for AspNetDependencyInjection package (e.g. '2.0.0-beta01' or '2.0.1'): 
echo Note that packages are sorted by the 3 version numbers first, then by the suffix in reverse alphabetical order, so `beta1` will be considered newer than `beta10`, so use a leading zero, e.g. `beta01`.

set /p uwfversion="Semver package version number: "

@echo on

nuget pack AspNetDependencyInjection.nuspec -OutputDirectory ..\..\Unity.WebForms_local\nuget-output -BasePath AspNetDependencyInjection -symbols -Version %uwfversion%

nuget pack AspNetDependencyInjection.Mvc.nuspec -OutputDirectory ..\..\Unity.WebForms_local\nuget-output -BasePath AspNetDependencyInjection.Mvc -symbols -Version %uwfversion%

REM TODO: Automatically publish to nuget.org using `nuget.exe push` - but how do I update the package README like how the web upload interface does?
