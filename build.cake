// Tools needed by Cake addins
#tool nuget:?package=vswhere&version=2.8.4

// Cake addins
#addin nuget:?package=Newtonsoft.Json&version=12.0.3
#addin nuget:?package=Cake.FileHelpers&version=3.2.1
#addin nuget:?package=Cake.MonoApiTools&version=3.0.5

var VISUAL_STUDIO_ROOT = EnvironmentVariable ("VISUAL_STUDIO_ROOT") ?? Argument ("vs", "");
if (string.IsNullOrEmpty(VISUAL_STUDIO_ROOT))
    VISUAL_STUDIO_ROOT = IsRunningOnWindows()
        ? VSWhereLatest(new VSWhereLatestSettings { Requires = "Component.Xamarin", IncludePrerelease = true }).FullPath
        : "/Applications/Visual Studio.app/Contents/MacOS/VisualStudio";

var VSTOOL_PATH = !IsRunningOnWindows() ? new FilePath(VISUAL_STUDIO_ROOT).GetDirectory().CombineWithFilePath("vstool").FullPath : "";

Information ($"VISUAL_STUDIO_ROOT   : {VISUAL_STUDIO_ROOT}");
Information ($"VSTOOL_PATH          : {VSTOOL_PATH}");

var target = Argument("target", "Clean");
var configuration = Argument("configuration", "Release");

var allConfigurations = new string[] {"Debug", "ReleaseWithDebugNativeRef", "Release"};

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    //.WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    foreach (var config in allConfigurations)
    {
        CleanDirectories($"./src/**/{config}");
        CleanDirectories($"./samples/**/{config}");
        CleanDirectories($"./toolsSrc/**/{config}");
    }
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreBuild("./src/Flutnet.Interop.iOS/Flutnet.Interop.iOS.csproj", new DotNetCoreBuildSettings
    {
        Configuration = configuration,
    });
});

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCoreTest("./src/Example.sln", new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true,
    });
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);