using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Resources;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle( "Unity.WebForms" )]
[assembly: AssemblyDescription( "Dependency Injection in ASP.NET WebForms using Unity" )]
[assembly: AssemblyConfiguration( "" )]
[assembly: AssemblyCompany( "" )]
[assembly: AssemblyProduct( "Unity.WebForms" )]
[assembly: AssemblyCopyright( "Copyright © 2013 S. Kyle Korndoerfer" )]
[assembly: AssemblyTrademark( "" )]
[assembly: AssemblyCulture( "" )]
[assembly: NeutralResourcesLanguageAttribute( "en-US" )]

// Register the HttpModule on application start
[assembly: PreApplicationStartMethod( typeof(Unity.WebForms.PreApplicationStart), "PreStart" )]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

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
[assembly: AssemblyVersion( "1.2.*" )]
[assembly: AssemblyFileVersion( "1.2.0.0" )]

