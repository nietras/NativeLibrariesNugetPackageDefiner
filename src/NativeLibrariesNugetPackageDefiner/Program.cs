using System;
using System.Diagnostics;
using static NativeLibrariesNugetPackageDefiner.NugetPackageDefiner;

Action<string> log = t => { Trace.WriteLine(t); Console.WriteLine(t); };

var author = "nietras";
var baseDirectory = "../../../../../";
var nativeLibsForPackageDirectory = baseDirectory + "libs";
var outputDirectory = baseDirectory + "defs";
var versionSuffix = "-preview.1";
// Native library files only so choosing very low TFM
var targetFrameworkMoniker = ".netstandard1.1";

FindNativeLibrariesThenDefinePackages(nativeLibsForPackageDirectory, author,
    targetFrameworkMoniker, versionSuffix, outputDirectory, log);
