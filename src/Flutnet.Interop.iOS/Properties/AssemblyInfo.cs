using System.Reflection;
using Foundation;

// This attribute allows you to mark your assemblies as “safe to link”.
// When the attribute is present, the linker—if enabled—will process the assembly
// even if you’re using the “Link SDK assemblies only” option, which is the default for device builds.
[assembly: LinkerSafe]

[assembly: AssemblyTitle("Flutnet.Interop.iOS")]
[assembly: AssemblyDescription("Xamarin.iOS bindings for Flutter engine's iOS embedding APIs")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("Novagem Solutions")]
[assembly: AssemblyProduct("Flutnet.Interop")]
[assembly: AssemblyCopyright("Copyright © Novagem Solutions S.r.l. 2020")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]


// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.
//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]
