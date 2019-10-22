REM TODO: Add check that nuget.exe is installed.

@echo off
echo Enter semver version number for AspNetDependencyInjection package (e.g. '2.0.0-beta01' or '2.0.1'): 
echo Note that packages are sorted by the 3 version numbers first, then by the suffix in reverse alphabetical order, so `beta1` will be considered newer than `beta10`, so use a leading zero, e.g. `beta01`.

set /p packageVersion="Semver package version number: "

@echo on

nuget pack AspNetDependencyInjection.nuspec -OutputDirectory ..\..\Unity.WebForms_local\nuget-output -BasePath AspNetDependencyInjection -symbols -Version %packageVersion%

REM adjust the dependency version:

REM ASP.NET MVC:

rxrepl.exe --file AspNetDependencyInjection.Mvc.nuspec --alter --search "<dependency id=""Jehoel.AspNetDependencyInjection""\s+version=""([^""]+)"" />" --replace "<dependency id=""Jehoel.AspNetDependencyInjection"" version=""%packageVersion%"" />" --no-backup

nuget pack AspNetDependencyInjection.Mvc.nuspec -OutputDirectory ..\..\Unity.WebForms_local\nuget-output -BasePath AspNetDependencyInjection.Mvc -symbols -Version %packageVersion%

REM ASP.NET SignalR:

rxrepl.exe --file AspNetDependencyInjection.SignalR.nuspec --alter --search "<dependency id=""Jehoel.AspNetDependencyInjection""\s+version=""([^""]+)"" />" --replace "<dependency id=""Jehoel.AspNetDependencyInjection"" version=""%packageVersion%"" />" --no-backup

nuget pack AspNetDependencyInjection.SignalR.nuspec -OutputDirectory ..\..\Unity.WebForms_local\nuget-output -BasePath AspNetDependencyInjection.SignalR -symbols -Version %packageVersion%

REM ASP.NET Web API:

rxrepl.exe --file AspNetDependencyInjection.WebApi.nuspec --alter --search "<dependency id=""Jehoel.AspNetDependencyInjection""\s+version=""([^""]+)"" />" --replace "<dependency id=""Jehoel.AspNetDependencyInjection"" version=""%packageVersion%"" />" --no-backup

nuget pack AspNetDependencyInjection.WebApi.nuspec -OutputDirectory ..\..\Unity.WebForms_local\nuget-output -BasePath AspNetDependencyInjection.WebApi -symbols -Version %packageVersion%

REM TODO: Automatically publish to nuget.org using `nuget.exe push` - but how do I update the package README like how the web upload interface does?
