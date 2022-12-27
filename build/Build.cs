using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
[GitHubActions(
    "nugetPublish",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = new[] { "main" },
    EnableGitHubToken = true,
    PublishArtifacts = true,
    ImportSecrets = new[] { nameof(NUGETAPIKEY) },
    InvokedTargets = new[] { nameof(Push) })]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    GitHubActions GitHubActions => GitHubActions.Instance;
    public static int Main() => Execute<Build>(x => x.Push);
    [Solution]
    readonly Solution Solution;
    AbsolutePath SourceDirectory => RootDirectory / "Source";
    AbsolutePath TestDirectory => RootDirectory / "Test";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    [Parameter]
    string NugetApiUrl = "https://api.nuget.org/v3/index.json";
    [Parameter]
    string GitPkgNugetApiUrl = "https://nuget.pkg.github.com/raminhz90/index.json";
    [Parameter] [Secret]
    readonly string NUGETAPIKEY;
    readonly Configuration Configuration =  Configuration.Release;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetTasks.DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });
    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTasks.DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild());
        });
    Target Pack => _ => _
        .DependsOn(Test)
        .Produces(ArtifactsDirectory / "*.nupkg")
        .Executes(() =>
        {
            DotNetTasks.DotNetPack(s => s
                .SetProject(Solution)
                .SetOutputDirectory(ArtifactsDirectory)
                .SetIncludeSymbols(true)
                .SetConfiguration(Configuration)
                .EnableNoRestore()
                .EnableNoBuild());
        });
    Target Push => _ => _
        .DependsOn(Pack)
        .Requires(() => NugetApiUrl)
        .Requires(() => NUGETAPIKEY)
        .Requires(() => Configuration.Equals(Configuration.Release))
        .Executes(() =>
        {
            GlobFiles(ArtifactsDirectory, "*.nupkg")
                .NotEmpty()
                .Where(x => !x.EndsWith("symbols.nupkg",StringComparison.Ordinal))
                .ForEach(x =>
                {
                    DotNetTasks.DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSkipDuplicate(true)
                        .SetSource(GitPkgNugetApiUrl)
                        .SetApiKey(GitHubActions.Token)
                    );
                    DotNetTasks.DotNetNuGetPush(s => s
                        .SetTargetPath(x)
                        .SetSkipDuplicate(true)
                        .SetSource(NugetApiUrl)
                        .SetApiKey(NUGETAPIKEY)
                    );
                });
        });

}
