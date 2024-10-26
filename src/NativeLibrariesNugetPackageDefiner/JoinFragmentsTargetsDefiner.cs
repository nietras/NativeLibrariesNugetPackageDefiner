namespace NativeLibrariesNugetPackageDefiner;

static class JoinFragmentsTargetsDefiner
{
    public static string JoinFragmentsTargets(string targetNameSuffix,
        string fileToJoin, string taskCSharpRelativePath,
        string runtimeIdentifier, int fragmentsCount,
        long expectedSize, string expectedHash) =>
    $"""
    <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
        <UsingTask TaskName="JoinFragmentsTask"
                   TaskFactory="RoslynCodeTaskFactory"
                   AssemblyFile="$(MSBuildToolsPath)\Microsoft.Build.Tasks.Core.dll">
            <Task>
                <Code Type="Class" Language="cs" Source="$(MSBuildThisFileDirectory)../../{taskCSharpRelativePath}" />
            </Task>
        </UsingTask>

        <Target Name="RunJoinFragmentsTask-{targetNameSuffix}"
                AfterTargets="ResolveReferences"
                BeforeTargets="PrepareForBuild">
            <JoinFragmentsTask FileToJoinFragmentsFor="$(MSBuildThisFileDirectory)../../runtimes/{runtimeIdentifier}/native/{fileToJoin}"
                               FragmentsCount="{fragmentsCount}"
                               ExpectedSize="{expectedSize}"
                               ExpectedHash="{expectedHash}" />
        </Target>
    </Project>
    """;
}
