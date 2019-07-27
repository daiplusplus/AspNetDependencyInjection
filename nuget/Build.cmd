REM TODO: Add check that nuget.exe is installed.

@echo off
set /p uwfversion="Enter semver version number for Unity.WebForms package (e.g. '2.0.0-beta1' or '2.0.1'): "

@echo on

nuget pack Unity.Webforms.nuspec -OutputDirectory ..\..\Unity.WebForms_local\nuget-output -symbols -Version %uwfversion%

REM TODO: Automatically publish to nuget.org using `nuget.exe push` - but how do I update the package README like how the web upload interface does?
