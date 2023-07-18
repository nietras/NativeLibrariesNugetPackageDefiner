using System;
using System.Diagnostics;
using static NativeLibrariesNugetPackageDefiner.NugetPackageDefiner;

Action<string> log = t => { Trace.WriteLine(t); Console.WriteLine(t); };

var baseDirectory = "../../../../../";
var nativeLibsForPackageDirectory = baseDirectory + "libs";
var outputDirectory = baseDirectory + "defs";
// Native library files only so choosing very low TFM
var targetFrameworkMoniker = ".netstandard1.1";

FindNativeLibrariesThenDefinePackages(nativeLibsForPackageDirectory,
    targetFrameworkMoniker, outputDirectory, log);
