using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace NativeLibrariesNugetPackageDefiner;

static class NugetPackageDefiner
{
    const int NugetPackageSizeMax = 250_000_000;
    const char IdSeparator = '.';
    const string IdRuntimePrefix = "runtime.";

    // File versioning is a joke, so need some other way to version the files.
    // Perhaps presence of `.version` file can be used and if none try to get from `FileVersionInfo`.
    // get-childitem * -include *.dll,*.exe | foreach-object { "{0}`t{1}" -f $_.Name, [System.Diagnostics.FileVersionInfo]::GetVersionInfo($_).FileVersion }

    // Probably put in directory structure
    //const string LicenseText = "This software contains source code provided by NVIDIA Corporation.\r\nThe full cuDNN license agreement can be found here.\r\nhttps://docs.nvidia.com/deeplearning/cudnn/sla/index.html";

    public static void FindNativeLibrariesThenDefinePackages(string nativeLibsForPackageDirectory,
        string author, string targetFrameworkMoniker, string versionSuffix,
        string outputDirectory, Action<string> log)
    {
        var basePackageIdentifierExtensionToPackageInfos = new Dictionary<string, List<PackageInfo>>();

        var packageDirectories = Directory.GetDirectories(nativeLibsForPackageDirectory);
        foreach (var packageDirectory in packageDirectories)
        {
            var rootPackageIdentifier = Path.GetRelativePath(nativeLibsForPackageDirectory, packageDirectory)
                .Replace(Path.DirectorySeparatorChar, IdSeparator).Replace(Path.AltDirectorySeparatorChar, IdSeparator);

            var metaPackageToPackageNames = Directory.GetFiles(packageDirectory, "*.meta.txt").ToDictionary(
                f => GetMetaPackageName(f, rootPackageIdentifier),
                f => File.ReadAllLines(f).Select(l => rootPackageIdentifier + IdSeparator + l).ToArray());

            var runtimeIdentifierDirectories = Directory.GetDirectories(packageDirectory);
            foreach (var runtimeIdentifierDirectory in runtimeIdentifierDirectories)
            {
                var runtimeIdentifier = Path.GetRelativePath(packageDirectory, runtimeIdentifierDirectory)
                    .Replace(Path.DirectorySeparatorChar, IdSeparator).Replace(Path.AltDirectorySeparatorChar, IdSeparator);

                // A specific runtime identifier should only contain one type of
                // native library file extension, so searching one at a time if
                // previous found nothing.
                var packageFiles = Directory.GetFiles(runtimeIdentifierDirectory, "*.dll");
                packageFiles = packageFiles.Length == 0 ? Directory.GetFiles(runtimeIdentifierDirectory, "*.so") : packageFiles;
                packageFiles = packageFiles.Length == 0 ? Directory.GetFiles(runtimeIdentifierDirectory, "*.dylib") : packageFiles;

                foreach (var packageFile in packageFiles)
                {
                    var fileName = Path.GetFileName(packageFile);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(packageFile);
                    var fileInfo = new FileInfo(packageFile);
                    var versionInfo = FileVersionInfo.GetVersionInfo(packageFile);
                    var version = Version.Parse(versionInfo.FileVersion!);
                    var nativeLibrarySize = (int)fileInfo.Length;

                    log($"Found '{packageFile}' size {nativeLibrarySize} version {version}"); //versionInfo {versionInfo}");

                    var basePackageIdentifier = $"{rootPackageIdentifier}.{fileNameWithoutExtension}";
                    var runtimeSpecificPackageIdentifier = $"{basePackageIdentifier}.{IdRuntimePrefix}{runtimeIdentifier}";
                    var nuspecDirectory = Path.Combine(outputDirectory, runtimeSpecificPackageIdentifier);
                    var packageVersion = version.ToString() + versionSuffix;

                    var splitSegments = EstimateSplitSegments(packageFile, nativeLibrarySize);

                    var fragments = new List<string>();
                    if (splitSegments.Count > 1)
                    {
                        var fragmentCount = splitSegments.Count;

                        // Main package will have an empty file, all the rest as
                        // fragments, this to ensure easy to spot that main has
                        // not been joined.
                        log($"Splitting '{packageFile}' into {fragmentCount} fragments");
                        for (int fragmentIndex = 0; fragmentIndex < splitSegments.Count; fragmentIndex++)
                        {
                            var fragment = $"fragment{fragmentIndex:D2}";
                            var (fragmentOffset, fragmentSize) = splitSegments[fragmentIndex];

                            var fragmentNuspecContents = NuspecDefiner.RuntimeSpecificFragmentPackage(
                                runtimeSpecificPackageIdentifier, packageVersion, author,
                                fragment, fragmentIndex, fragmentCount);

                            var fragmentPackageIdentifier = NuspecDefiner.FragmentPackageId(
                                runtimeSpecificPackageIdentifier, fragment);
                            var fragmentPackageDirectory = Path.Combine(outputDirectory, fragmentPackageIdentifier);
                            WriteNuspec(fragmentNuspecContents, fragmentPackageDirectory, fragmentPackageIdentifier);

                            var fragmentFileName = $"{fileName}.{fragment}";
                            var fragmentDirectory = Path.Combine(fragmentPackageDirectory,
                                "fragments", runtimeIdentifier, "native");
                            var fragmentPath = Path.Combine(fragmentDirectory, fragmentFileName);
                            EnsureDirectoryCreated(fragmentDirectory);
                            CopyFileSegmentTo(packageFile, fragmentOffset, fragmentSize, fragmentPath);

                            var fragmentReadmeContents = "Runtime identifier specific fragment package for native library file too big for a single package on nuget.";
                            var fragmentReadmePath = Path.Combine(fragmentPackageDirectory, "README.md");
                            File.WriteAllText(fragmentReadmePath, fragmentReadmeContents);

                            fragments.Add(fragment);
                        }
                    }

                    // Main package will have an empty file if there are
                    // fragments, otherwise it is the full file, of course.
                    var (mainPackageOffset, mainPackageSize) =
                        splitSegments.Count != 1 ? new(0, 0) : splitSegments[0];

                    var nuspecContents = NuspecDefiner.RuntimeSpecificPackage(
                        runtimeSpecificPackageIdentifier, packageVersion, author, fragments);
                    WriteNuspec(nuspecContents, nuspecDirectory, runtimeSpecificPackageIdentifier);

                    var nativeLibraryRelativeDirectory = Path.Combine("runtimes", runtimeIdentifier, "native");
                    var nativeLibraryRelativePath = Path.Combine(nativeLibraryRelativeDirectory, fileName);
                    var nativeLibraryDirectory = Path.Combine(nuspecDirectory, nativeLibraryRelativeDirectory);
                    var nativeLibraryPath = Path.Combine(nuspecDirectory, nativeLibraryRelativePath);
                    EnsureDirectoryCreated(nativeLibraryDirectory);
                    CopyFileSegmentTo(packageFile, mainPackageOffset, mainPackageSize, nativeLibraryPath);

                    var readmeContents = "Runtime identifier specific package for native library file. Possibly split into multiple fragments or parts if size too large for nuget.";
                    var readmePath = Path.Combine(nuspecDirectory, "README.md");
                    File.WriteAllText(readmePath, readmeContents);

                    if (fragments.Count > 0)
                    {
                        var hash = ComputeHashSHA256(packageFile);

                        var joinFragmentsRelativeDirectory = "tasks";
                        var joinFragmentsTaskFileName = "JoinFragmentsTask.cs";
                        var joinFragmentsTaskRelativePath = Path.Combine(joinFragmentsRelativeDirectory, joinFragmentsTaskFileName);

                        // Write .targets file for MSBuild to use to join fragments
                        var targetsDirectory = Path.Combine(nuspecDirectory, "buildTransitive", targetFrameworkMoniker);
                        var targetsContents = JoinFragmentsTargetsDefiner.JoinFragmentsTargets(fileName,
                            joinFragmentsTaskRelativePath, runtimeIdentifier, fragments.Count, nativeLibrarySize, hash);
                        var targetsPath = Path.Combine(targetsDirectory, $"{runtimeSpecificPackageIdentifier}.targets");
                        EnsureDirectoryCreated(targetsDirectory);
                        File.WriteAllText(targetsPath, targetsContents);

                        // Write the C# file defining the MSBuild task to join fragments
                        var joinFragmentsTaskDirectory = Path.Combine(nuspecDirectory, joinFragmentsRelativeDirectory);
                        var joinFragmentsTaskContents = ReadResourceNameEndsWith(joinFragmentsTaskFileName);
                        var joinFragmentsTaskPath = Path.Combine(joinFragmentsTaskDirectory, joinFragmentsTaskFileName);
                        EnsureDirectoryCreated(joinFragmentsTaskDirectory);
                        File.WriteAllText(joinFragmentsTaskPath, joinFragmentsTaskContents);
                    }

                    if (!basePackageIdentifierExtensionToPackageInfos.TryGetValue(basePackageIdentifier, out var packageInfos))
                    {
                        packageInfos = new();
                        basePackageIdentifierExtensionToPackageInfos.Add(basePackageIdentifier, packageInfos);
                    }
                    packageInfos.Add(new(runtimeSpecificPackageIdentifier, packageVersion, runtimeIdentifier));
                }
            }

            // Define meta packages for easier consuming
            var suffixToPackageInfos = basePackageIdentifierExtensionToPackageInfos.Values
                .SelectMany(i => i)
                .GroupBy(i => i.RuntimeIdentifier)
                .ToDictionary(g => IdSeparator + IdRuntimePrefix + g.Key, g => g.ToList());

            var toProcess = new Queue<KeyValuePair<string, string[]>>(metaPackageToPackageNames);
            while (toProcess.TryDequeue(out var metaPackageAndPackageNames))
            {
                var (metaName, packageNames) = metaPackageAndPackageNames;
                bool foundPackages = false;
                foreach (var (suffix, packageInfos) in suffixToPackageInfos)
                {
                    var metaPackageInfos = packageInfos
                        .Where(info => packageNames.Any(metaPackageName => info.Name.StartsWith(metaPackageName)))
                        .ToArray();
                    if (metaPackageInfos.Length > 0 && metaPackageInfos.Length == packageNames.Length)
                    {
                        var metaPackageIdentifier = metaName + suffix;

                        // HACK: Use first version for meta package version
                        var version = metaPackageInfos[0].Version;
                        var nuspecDirectory = Path.Combine(outputDirectory, metaPackageIdentifier);
                        var nuspecContents = NuspecDefiner.MetaPackage(metaPackageIdentifier, version, author, metaPackageInfos);
                        WriteNuspec(nuspecContents, nuspecDirectory, metaPackageIdentifier);

                        var readmeContents = "Meta package for a set of native libraries.";
                        var readmePath = Path.Combine(nuspecDirectory, "README.md");
                        File.WriteAllText(readmePath, readmeContents);

                        // Add to suffixes to process
                        if (!suffixToPackageInfos.TryGetValue(suffix, out var infos))
                        {
                            infos = new List<PackageInfo>();
                            suffixToPackageInfos.Add(suffix, infos);
                        }
                        infos.Add(new(metaPackageIdentifier, version, ""));

                        foundPackages = true;
                        break;
                    }
                }
                if (!foundPackages)
                {
                    // Try again later
                    toProcess.Enqueue(metaPackageAndPackageNames);
                }
            }

            // Define runtime agnostic runtime.json meta packages
            foreach (var (basePackageIdentifier, infos) in basePackageIdentifierExtensionToPackageInfos)
            {
                var packageIdentifier = basePackageIdentifier + IdSeparator + IdRuntimePrefix + "json";

                // HACK: Use first version for meta package version
                var version = infos[0].Version;
                var nuspecDirectory = Path.Combine(outputDirectory, packageIdentifier);
                var nuspecContents = NuspecDefiner.MetaPackage(packageIdentifier, version, author);
                WriteNuspec(nuspecContents, nuspecDirectory, packageIdentifier);

                // Empty file might be needed to silence nuget if no assembly/lib file
                // Only use in meta package
                var emptyTargetFrameworkLibDirectory = Path.Combine(nuspecDirectory, $"lib/{targetFrameworkMoniker}/");
                var emptyTargetFrameworkLibPath = Path.Combine(emptyTargetFrameworkLibDirectory, $"_._");
                EnsureDirectoryCreated(emptyTargetFrameworkLibDirectory);
                File.WriteAllText(emptyTargetFrameworkLibPath, string.Empty);

                var readmeContents = "Meta runtime.json package for native library file split into runtime identifier specific packages.";
                var readmePath = Path.Combine(nuspecDirectory, "README.md");
                File.WriteAllText(readmePath, readmeContents);

                WriteRuntimeJson(basePackageIdentifier, infos, Path.Combine(nuspecDirectory, "runtime.json"));
            }
        }
    }

    static string GetMetaPackageName(string filePath, string rootPackageIdentifier)
    {
        var fileName = Path.GetFileName(filePath.Replace(".meta.txt", ""));
        var metaSuffix = (fileName.Length > 0 ? IdSeparator + fileName : string.Empty);
        return rootPackageIdentifier + metaSuffix;
    }

    static List<(long Offset, long Count)> EstimateSplitSegments(string packageFile, int nativeLibrarySize)
    {
        var splitSegments = new List<(long Offset, long Count)>();
        if (nativeLibrarySize < NugetPackageSizeMax)
        {
            splitSegments.Add((0, nativeLibrarySize));
        }
        else
        {
            const int blockSize = 8 * 1024 * 1024;
            const int safeMargin = 16 * 1024;

            // Estimate split sizes by incrementally gzip'ing the file
            var buffer = new byte[blockSize];
            using (var src = new FileStream(packageFile, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: blockSize))
            using (var ms = new MemoryStream(NugetPackageSizeMax))
            // Not using CompressionLevel.SmallestSize due to speed
            // and to get conservative estimate. TODO: Consider
            // compress blocks in parallel for speed trading off
            // worse estimate.
            using (var gz = new GZipStream(ms, CompressionLevel.Fastest))
            {
                var prevOffset = 0;
                var offset = 0;
                while (offset < nativeLibrarySize)
                {
                    var count = src.ReadAtLeast(buffer, blockSize, throwOnEndOfStream: false);
                    if (count <= 0) { break; }

                    gz.Write(buffer, 0, count);
                    gz.Flush();

                    offset += count;
                    if (ms.Length > (NugetPackageSizeMax - blockSize - safeMargin))
                    {
                        splitSegments.Add((prevOffset, offset - prevOffset));
                        prevOffset = offset;
                        ms.Position = 0;
                        ms.SetLength(0);
                    }
                }
                splitSegments.Add((prevOffset, offset - prevOffset));
            }
        }
        return splitSegments;
    }

    static void WriteNuspec(string contents, string directory, string packageIdentifier)
    {
        var nuspecFileName = packageIdentifier + ".nuspec";
        var nuspecPath = Path.Combine(directory, nuspecFileName);
        // meta package must be created after for each unique file name
        EnsureDirectoryCreated(directory);
        File.WriteAllText(nuspecPath, contents);
    }

    static void EnsureDirectoryCreated(string directory)
    {
        if (!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }
    }

    static void CopyFileSegmentTo(string srcPath, long offset, long count, string dstPath)
    {
        var bufferSize = 8 * 1024 * 1024;
        using var src = new FileStream(srcPath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize);
        using var dst = new FileStream(dstPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize);
        src.Position = offset;
        var buffer = new byte[bufferSize];
        int read = 0;
        while (count > 0 &&
               (read = src.Read(buffer, 0, (int)Math.Min(buffer.Length, count))) > 0)
        {
            dst.Write(buffer, 0, read);
            count -= read;
        }
    }

    static string ComputeHashSHA256(string path)
    {
        using var stream = File.OpenRead(path);
        using var hash = System.Security.Cryptography.SHA256.Create();
        return BitConverter.ToString(hash.ComputeHash(stream));
    }

    static string ReadResourceNameEndsWith(string endString)
    {
        var assembly = typeof(NugetPackageDefiner).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        var resourceName = resourceNames.Single(x => x.EndsWith(endString));
        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // Should any remaining runtimes not currently avaliable as dlls simply be
    // added, to keep this open to being fixed even for old versions in the future?
    static void WriteRuntimeJson(string basePackageIdentifier, List<PackageInfo> infos, string filePath)
    {
        var header = "{\r\n  \"runtimes\": {\r\n";
        var footer = "  }\r\n}\r\n";
        var sb = new StringBuilder();
        sb.Append(header);
        foreach (var info in infos)
        {
            sb.AppendLine(
                $$"""
                "{{info.RuntimeIdentifier}}": {
                  "{{basePackageIdentifier}}": {
                    "{{info.Name}}": "{{info.Version}}"
                  }
                },
            """);
        }
        // TODO: Add remaining runtimes with base version just in case for future
        sb.Append(footer);
        File.WriteAllText(filePath, sb.ToString());
    }
}
