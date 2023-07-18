using System;
using System.CommandLine;
using System.Diagnostics;
using static NativeLibrariesNugetPackageDefiner.NugetPackageDefiner;

Action<string> log = t => { Trace.WriteLine(t); Console.WriteLine(t); };

var inputDirectoryArgument = new Argument<string>(
    name: "input", description: "The directory to scan and define packages for.");

var outputDirectoryArgument = new Argument<string>(
    name: "output", description: "The directory to output package definitions in.");

var rootCommand = new RootCommand("Native libraries nuget package definer.");
rootCommand.AddArgument(inputDirectoryArgument);
rootCommand.AddArgument(outputDirectoryArgument);

rootCommand.SetHandler((inputDirectory, outputDirectory) =>
    {
        // Native library files only so choosing very low TFM
        var targetFrameworkMoniker = ".netstandard1.1";

        FindNativeLibrariesThenDefinePackages(inputDirectory,
            targetFrameworkMoniker, outputDirectory, log);
    },
    inputDirectoryArgument, outputDirectoryArgument);

return await rootCommand.InvokeAsync(args);