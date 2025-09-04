using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

// AssemblyTitle is the one that gets into file / properties / description and
// toolbar right click name via the registry
// It is stored in registry
// HKEY_CLASSES_ROOT\Local Settings\Software\Microsoft\Windows\Shell\MuiCache
// ... C:\Users\georg\Desktop\Go.exe.FriendlyAppName
// path being wherever the .exe is run from so
// ... C:\Users\georg\My Drive\repos\Go\Go\bin\Debug\Go.exe.FriendlyAppName
// If you change the AssemblyTitle you must delete all the Go.exe.FriendlyAppName with the old title
// to get the right effect.
// The search function in the regedit seems a bit hit and miss
// that us now automated in GetSetNames

[assembly: AssemblyTitle("Go Planner")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Go Planner Product")]
[assembly: AssemblyCopyright("Copyright ©  2025")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("46504d66-3d1e-48d8-a3a7-4188cb0edbc4")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("6.7.99")]
