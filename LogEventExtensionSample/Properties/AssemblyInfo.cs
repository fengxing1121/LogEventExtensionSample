using System.Reflection;
using System.Runtime.InteropServices;
using TcHmiSrv.Management;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("LogEventExtensionSample")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LogEventExtensionSample")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("18a05103-d9ca-46a7-b405-3d5c3c82ec8d")]

[assembly: // keep this newline
    AssemblyVersion("1.0.0.0")]
[assembly: // keep this newline
    AssemblyFileVersion("1.0.0.0")]

// Declare the default type of the server extension
[assembly: ExtensionType(typeof(LogEventExtensionSample.LogEventExtensionSample))]
