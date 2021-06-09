using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "AspNetDependencyInjection" )]
[assembly: AssemblyDescription( "Dependency Injection in \"Classic\" ASP.NET using Microsoft.Extensions.DependencyInjection." )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "" )]
[assembly: AssemblyProduct( "AspNetDependencyInjection" )]
[assembly: AssemblyCopyright( "Copyright © 2013 - 2019 S. Kyle Korndoerfer, Dai Rees (@Jehoel on GitHub)" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]
[assembly: NeutralResourcesLanguage( "en-US" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible( false )]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid( "26a7c154-c919-4169-bf4e-702aa5398e1c" )]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion( "4.0.*" )]
//[assembly: AssemblyFileVersion( "2.0.0.0" )]

// Version 1.x = S. Kyle Korndoerfer's Unity.WebForms.
// Version 2.x = Jehoel's Unity.WebForms using WebObjectActivator.
// Version 3.x = AspNetDependencyInjection using Microsoft.Extensions.DependencyInjection.
// Version 4.x = AspNetDependencyInjection using Microsoft.Extensions.DependencyInjection.

[assembly: InternalsVisibleTo( assemblyName: @"AspNetDependencyInjection.Tests, PublicKey=" +
	"0024000004800000940000000602000000240000525341310004000001000100b5644bd32f0714" +
	"dce56bb49d687880774726f85935f8213aff7d200ef6b75d6103996e8e26410b54475eeaeae3dd" +
	"3b1f48900025d8e66a8beb6c0580416ca82a0ef3ee4794dc2c06aa0d5e23ca5e4efaef2c98cc66" +
	"3efb5f00ef1c20b962a432e61dbbd5f672841d39de5b5ee2d157341d5d28a43c9bb7493ec838c8" +
	"e94586a5" )]
