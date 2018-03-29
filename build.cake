#tool "nuget:?package=GitVersion.CommandLine"

///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

DotNetCoreMSBuildSettings msBuildSettings = null;

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    var version = GitVersion();

    msBuildSettings = new DotNetCoreMSBuildSettings()
        .WithProperty("Version", version.LegacySemVerPadded)
        .WithProperty("AssemblyVersion", version.AssemblySemVer)
        .WithProperty("FileVersion", version.MajorMinorPatch)
        .WithProperty("InformationalVersion", version.InformationalVersion);
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectories("./artifacts");
});

Task("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    DotNetCoreRestore("./src/ServiceFabric.Services.Grpc/ServiceFabric.Services.Grpc.csproj",
        new DotNetCoreRestoreSettings {
            MSBuildSettings = msBuildSettings
        });
});

Task("Build")
    .IsDependentOn("Restore")
    .Does(() =>
{
    DotNetCoreBuild("./src/ServiceFabric.Services.Grpc/ServiceFabric.Services.Grpc.csproj",
        new DotNetCoreBuildSettings {
            MSBuildSettings = msBuildSettings,
            Configuration = configuration,
            NoRestore = true
        });
});

Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
{
    DotNetCorePack("./src/ServiceFabric.Services.Grpc/ServiceFabric.Services.Grpc.csproj",
        new DotNetCorePackSettings {
            MSBuildSettings = msBuildSettings,
            Configuration = configuration,
            NoRestore = true,
            OutputDirectory = "./artifacts"
        });
});

Task("Default")
    .IsDependentOn("Pack");

RunTarget(target);