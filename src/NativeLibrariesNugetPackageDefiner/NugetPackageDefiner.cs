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
    const string AuthorFileName = "author.txt";
    const string VersionFileName = "version.txt";
    const string ReadmeFileName = "README.md";
    const string LicenseFileName = "LICENSE.txt";
    const string ThirdPartyNoticesFileName = "ThirdPartyNotices.txt";

    const int NugetPackageSizeMax = 250_000_000;
    const char IdSeparator = '.';
    const string IdRuntimePrefix = "runtime.";
    // Needs to be short to try to keep file name length in check
    const string FragmentPrefix = "f";
    // On Linux or similar a native library might start with "lib", to unify
    // naming, remove this from start of package name.
    const string LibPrefix = "lib";
    // Some libraries will start with "lib" even if ".dll" so cannot remove lib then
    const string DllExtension = ".dll";
    // Using "json" as hacked runtime identifier to allow handling
    // `runtime.json` packages in same way as say `win-x64`.
    const string JsonRuntimeIdentifier = "json";
    // To document native libraries found and being packaged, without adding
    // them to git write size of native library found to a file next to where
    // the file found.
    const string SizeFileNameSuffix = ".size.txt";

    const string ReadmeContentsRuntimeSpecificFileFragment = "Runtime identifier specific fragment package for native library file too big for a single package on nuget.";
    const string ReadmeContentsRuntimeSpecificFile = "Runtime identifier specific package for native library file. Possibly split into multiple fragments or parts if size too large for nuget.";
    const string ReadmeContentsMetaRuntimeJson = "Meta runtime.json package for native library file split into runtime identifier specific packages.";
    const string ReadmeContentsMeta = "Meta package for a set of native libraries.";

    public static void FindNativeLibrariesThenDefinePackages(
        string inputDirectory, string targetFrameworkMoniker, string outputDirectory,
        Action<string> log)
    {
        var packageDirectories = Directory.GetDirectories(inputDirectory);
        foreach (var packageDirectory in packageDirectories)
        {
            var rootPackageIdentifier = Path.GetRelativePath(inputDirectory, packageDirectory)
                .Replace(Path.DirectorySeparatorChar, IdSeparator).Replace(Path.AltDirectorySeparatorChar, IdSeparator);

            var metaPackageToPackageNames = Directory.GetFiles(packageDirectory, "*.meta.txt").ToDictionary(
                f => GetMetaPackageName(f, rootPackageIdentifier),
                f => File.ReadAllLines(f).Select(l => l.Replace("<ROOT>", rootPackageIdentifier, StringComparison.OrdinalIgnoreCase)).ToArray());

            var author = CheckExistsReadAllText(packageDirectory, AuthorFileName,
                rootPackageIdentifier, "contain a single line with package author");

            var version = CheckExistsReadAllText(packageDirectory, VersionFileName,
                rootPackageIdentifier, "contain a single line with package version");

            var license = CheckExistsReadAllText(packageDirectory, LicenseFileName,
                rootPackageIdentifier, "contain license text");

            var thirdPartyNoticesFilePath = Path.Combine(packageDirectory, ThirdPartyNoticesFileName);
            var thirdPartyNotices = File.Exists(thirdPartyNoticesFilePath)
                ? File.ReadAllText(thirdPartyNoticesFilePath) : null;

            var basePackageIdentifierExtensionToPackageInfos = new Dictionary<string, List<PackageInfo>>();

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
                packageFiles = packageFiles.Length == 0 ? Directory.GetFiles(runtimeIdentifierDirectory, "*.aar") : packageFiles;
                // TODO: Handle iOS where there is no file extension

                foreach (var packageFile in packageFiles)
                {
                    var fileName = Path.GetFileName(packageFile);
                    var fileInfo = new FileInfo(packageFile);
                    var nativeLibrarySize = fileInfo.Length;

                    // Native libraries are very rarely properly versioned, so
                    // this is just to be able to double check against the
                    // defined version in version file.
                    var versionInfo = FileVersionInfo.GetVersionInfo(packageFile);

                    log($"Found '{packageFile}' size {nativeLibrarySize} " +
                        $"file version '{versionInfo.FileVersion}' defined version '{version}'");

                    var nativeLibrarySizeFilePath = Path.Combine(runtimeIdentifierDirectory, fileName + SizeFileNameSuffix);
                    WriteTextIfDifferent(nativeLibrarySizeFilePath, nativeLibrarySize.ToString());

                    var extension = Path.GetExtension(packageFile);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(packageFile);
                    var sanitizedFileNameForPackageIdentifier = fileNameWithoutExtension
                        .IndexOf(LibPrefix, StringComparison.Ordinal) == 0 &&
                        // Only remove "lib" if not .dll, to keep it for .dll files
                        !extension.Equals(DllExtension, StringComparison.OrdinalIgnoreCase)
                        ? fileNameWithoutExtension.Substring(LibPrefix.Length) : fileNameWithoutExtension;

                    var basePackageIdentifier = $"{rootPackageIdentifier}.{sanitizedFileNameForPackageIdentifier}";
                    var runtimeSpecificPackageIdentifier = $"{basePackageIdentifier}.{IdRuntimePrefix}{runtimeIdentifier}";
                    var nuspecDirectory = Path.Combine(outputDirectory, runtimeSpecificPackageIdentifier);

                    var splitSegments = EstimateSplitSegments(packageFile, nativeLibrarySize, log).ToList();

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
                            var fragment = $"{FragmentPrefix}{fragmentIndex:D2}";
                            var (fragmentOffset, fragmentSize) = splitSegments[fragmentIndex];

                            var fragmentNuspecContents = NuspecDefiner.RuntimeSpecificFragmentPackage(
                                runtimeSpecificPackageIdentifier, version, author,
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

                            var fragmentReadmeContents = ReadmeContentsRuntimeSpecificFileFragment;
                            WriteReadmeLicenseMaybeThirdPartyNotices(fragmentPackageDirectory,
                                fragmentReadmeContents, license, thirdPartyNotices);

                            fragments.Add(fragment);
                        }
                    }

                    // Main package will have an empty file if there are
                    // fragments, otherwise it is the full file, of course.
                    var (mainPackageOffset, mainPackageSize) =
                        splitSegments.Count != 1 ? new(0, 0) : splitSegments[0];

                    var nuspecContents = NuspecDefiner.RuntimeSpecificPackage(
                        runtimeSpecificPackageIdentifier, version, author, fragments);
                    WriteNuspec(nuspecContents, nuspecDirectory, runtimeSpecificPackageIdentifier);

                    var nativeLibraryRelativeDirectory = Path.Combine("runtimes", runtimeIdentifier, "native");
                    var nativeLibraryRelativePath = Path.Combine(nativeLibraryRelativeDirectory, fileName);
                    var nativeLibraryDirectory = Path.Combine(nuspecDirectory, nativeLibraryRelativeDirectory);
                    var nativeLibraryPath = Path.Combine(nuspecDirectory, nativeLibraryRelativePath);
                    EnsureDirectoryCreated(nativeLibraryDirectory);
                    CopyFileSegmentTo(packageFile, mainPackageOffset, mainPackageSize, nativeLibraryPath);

                    var readmeContents = ReadmeContentsRuntimeSpecificFile;
                    WriteReadmeLicenseMaybeThirdPartyNotices(nuspecDirectory,
                        readmeContents, license, thirdPartyNotices);

                    if (fragments.Count > 0)
                    {
                        var hash = ComputeHashSHA256(packageFile);

                        var joinFragmentsRelativeDirectory = "tasks";
                        var joinFragmentsTaskFileName = "JoinFragmentsTask.cs";
                        var joinFragmentsTaskRelativePath = Path.Combine(joinFragmentsRelativeDirectory, joinFragmentsTaskFileName);

                        // Write .targets file for MSBuild to use to join fragments
                        var targetsDirectory = Path.Combine(nuspecDirectory, "buildTransitive", targetFrameworkMoniker);
                        // Target name cannot contain '.'
                        var targetNameSuffix = runtimeSpecificPackageIdentifier.Replace('.', '_');
                        var targetsContents = JoinFragmentsTargetsDefiner.JoinFragmentsTargets(targetNameSuffix, fileName,
                            joinFragmentsTaskRelativePath, runtimeIdentifier, fragments.Count, nativeLibrarySize, hash);
                        var targetsPath = Path.Combine(targetsDirectory, $"{runtimeSpecificPackageIdentifier}.targets");
                        EnsureDirectoryCreated(targetsDirectory);
                        WriteTextIfDifferent(targetsPath, targetsContents);

                        // Write the C# file defining the MSBuild task to join fragments
                        var joinFragmentsTaskDirectory = Path.Combine(nuspecDirectory, joinFragmentsRelativeDirectory);
                        var joinFragmentsTaskContents = ReadResourceNameEndsWith(joinFragmentsTaskFileName);
                        var joinFragmentsTaskPath = Path.Combine(joinFragmentsTaskDirectory, joinFragmentsTaskFileName);
                        EnsureDirectoryCreated(joinFragmentsTaskDirectory);
                        WriteTextIfDifferent(joinFragmentsTaskPath, joinFragmentsTaskContents);
                    }

                    if (!basePackageIdentifierExtensionToPackageInfos.TryGetValue(basePackageIdentifier, out var packageInfos))
                    {
                        packageInfos = new();
                        basePackageIdentifierExtensionToPackageInfos.Add(basePackageIdentifier, packageInfos);
                    }
                    packageInfos.Add(new(runtimeSpecificPackageIdentifier, version, runtimeIdentifier));
                }
            }

            // Define runtime agnostic runtime.json meta packages (copy source to be able to add to it)
            foreach (var (basePackageIdentifier, infos) in basePackageIdentifierExtensionToPackageInfos.ToDictionary(p => p.Key, p => p.Value))
            {
                var packageIdentifier = basePackageIdentifier + IdSeparator + IdRuntimePrefix + JsonRuntimeIdentifier;

                // HACK: Use first version for meta package version
                var nuspecDirectory = Path.Combine(outputDirectory, packageIdentifier);
                var nuspecContents = NuspecDefiner.MetaPackage(packageIdentifier, version, author);
                WriteNuspec(nuspecContents, nuspecDirectory, packageIdentifier);

                // Empty file might be needed to silence nuget if no assembly/lib file
                // Only use in meta package
                var emptyTargetFrameworkLibDirectory = Path.Combine(nuspecDirectory, $"lib/{targetFrameworkMoniker}/");
                var emptyTargetFrameworkLibPath = Path.Combine(emptyTargetFrameworkLibDirectory, $"_._");
                EnsureDirectoryCreated(emptyTargetFrameworkLibDirectory);
                WriteTextIfDifferent(emptyTargetFrameworkLibPath, string.Empty);

                var readmeContents = ReadmeContentsMetaRuntimeJson;
                WriteReadmeLicenseMaybeThirdPartyNotices(nuspecDirectory,
                    readmeContents, license, thirdPartyNotices);

                WriteRuntimeJson(packageIdentifier, infos, Path.Combine(nuspecDirectory, "runtime.json"));

                basePackageIdentifierExtensionToPackageInfos[basePackageIdentifier].Add(new(packageIdentifier, version, JsonRuntimeIdentifier));
            }


            // Define meta packages for easier consuming
            // TODO: How to define cross-library meta packages?
            //       Need to move this one loop out, and handle differently?
            //       What about external dependencies in meta, some way to handle that?
            var suffixToPackageInfos = basePackageIdentifierExtensionToPackageInfos.Values
                .SelectMany(i => i)
                .GroupBy(i => i.RuntimeIdentifier)
                .ToDictionary(g => IdSeparator + IdRuntimePrefix + g.Key, g => g.ToList());

            var toProcess = new Queue<KeyValuePair<string, string[]>>(metaPackageToPackageNames);
            while (toProcess.TryDequeue(out var metaPackageAndPackageNames))
            {
                var (metaName, packageNames) = metaPackageAndPackageNames;
                var metaPackageCount = suffixToPackageInfos.Count;
                foreach (var (suffix, packageInfos) in suffixToPackageInfos)
                {
                    var metaPackageInfos = packageInfos
                        .Where(info => packageNames.Any(metaPackageName => info.Name.StartsWith(metaPackageName)))
                        .ToArray();
                    // Doesn't have to be same length as expected since in some
                    // cases some runtime identifier specific packages don't
                    // have all of the expected packages e.g. for ONNX runtime
                    // `android` does not have `onnxruntime_shared_providers`.
                    if (metaPackageInfos.Length > 0)
                    {
                        var metaPackageIdentifier = metaName + suffix;

                        // HACK: Use first version for meta package version
                        var nuspecDirectory = Path.Combine(outputDirectory, metaPackageIdentifier);
                        var nuspecContents = NuspecDefiner.MetaPackage(metaPackageIdentifier, version, author, metaPackageInfos);
                        WriteNuspec(nuspecContents, nuspecDirectory, metaPackageIdentifier);

                        var readmeContents = ReadmeContentsMeta;
                        WriteReadmeLicenseMaybeThirdPartyNotices(nuspecDirectory,
                            readmeContents, license, thirdPartyNotices);

                        // Add to suffixes to process
                        if (!suffixToPackageInfos.TryGetValue(suffix, out var infos))
                        {
                            infos = new List<PackageInfo>();
                            suffixToPackageInfos.Add(suffix, infos);
                        }
                        infos.Add(new(metaPackageIdentifier, version, string.Empty));

                        --metaPackageCount;
                    }
                }
                if (metaPackageCount > 0)
                {
                    if (toProcess.Count > 0)
                    {
                        log($"Try later '{metaPackageAndPackageNames.Key}' with '{string.Join(",", metaPackageAndPackageNames.Value)}'");
                        // Try again later
                        toProcess.Enqueue(metaPackageAndPackageNames);
                    }
                    else
                    {
                        log($"SKIPPING meta-package '{metaPackageAndPackageNames.Key}' due to missing dependencies (check names).");
                    }
                }
            }
        }
    }

    static string CheckExistsReadAllText(string directory, string fileName,
        string rootPackageIdentifier, string details)
    {
        var filePath = Path.Combine(directory, fileName);
        var version = File.Exists(filePath)
            ? File.ReadAllText(filePath)
            : throw new ArgumentException($"No '{fileName}' found for '{rootPackageIdentifier}'. " +
                $"This file must be present and {details}.");
        return version;
    }

    static string GetMetaPackageName(string filePath, string rootPackageIdentifier)
    {
        var fileName = Path.GetFileName(filePath.Replace(".meta.txt", ""));
        var metaSuffix = (fileName.Length > 0 ? IdSeparator + fileName : string.Empty);
        return rootPackageIdentifier + metaSuffix;
    }

    /// <summary>
    /// Estimate split sizes by incrementally zipping blocks of the file
    /// </summary>
    static IEnumerable<(long Offset, long Count)> EstimateSplitSegments(string filePath, long fileSize, Action<string> log)
    {
        if (fileSize < NugetPackageSizeMax)
        {
            yield return (0, fileSize);
        }
        else
        {
            log($"Estimating if splitting needed by compressing blocks for '{filePath}'");

            const int blockSize = 8 * 1024 * 1024;
            const int safeMargin = 16 * 1024;
            const int maxCompressedSize = NugetPackageSizeMax - blockSize - safeMargin;

            // Estimate compressed block sizes in parallel for speed.
            // Occurs a lot of memory pressure, but should be fine.
            // If not consider pooling.
            var compressedBlockSizes = EnumerateBlocks(filePath, blockSize)
                .AsParallel().AsOrdered()
                .Select(block => (block.Length, EstimateCompressedSize(block)))
                .ToList();

            var start = 0L;
            var end = 0L;
            var accumulatedCompressedBlockSize = 0L;
            foreach (var (originalBlockSize, compressedBlockSize) in compressedBlockSizes)
            {
                end += originalBlockSize;
                accumulatedCompressedBlockSize += compressedBlockSize;
                if (accumulatedCompressedBlockSize > maxCompressedSize)
                {
                    yield return (start, end - start);
                    start = end;
                    accumulatedCompressedBlockSize = 0;
                }
            }
            Debug.Assert(end == fileSize);
            yield return (start, end - start);
        }
    }

    static IEnumerable<Memory<byte>> EnumerateBlocks(string filePath, int blockSize)
    {
        using var src = new FileStream(filePath, FileMode.Open,
            FileAccess.Read, FileShare.Read, bufferSize: blockSize);
        {
            while (true)
            {
                var buffer = new byte[blockSize];
                var count = src.ReadAtLeast(buffer, blockSize, throwOnEndOfStream: false);
                if (count <= 0) { break; }
                yield return buffer.AsMemory(0, count);
            }
        }
    }

    static long EstimateCompressedSize(Memory<byte> block)
    {
        using var ms = new MemoryStream(NugetPackageSizeMax);
        // Not using CompressionLevel.SmallestSize due to speed and to get
        // conservative estimate.
        using var gz = new GZipStream(ms, CompressionLevel.Fastest);
        gz.Write(block.Span);
        gz.Flush();
        return ms.Length;
    }

    static void WriteNuspec(string contents, string directory, string packageIdentifier)
    {
        var nuspecFileName = packageIdentifier + ".nuspec";
        var nuspecPath = Path.Combine(directory, nuspecFileName);
        // meta package must be created after for each unique file name
        EnsureDirectoryCreated(directory);
        WriteTextIfDifferent(nuspecPath, contents);
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

    static void WriteReadmeLicenseMaybeThirdPartyNotices(
        string packageDirectory, string readmeContents, string licenseContents, string? thirdPartyNoticesContents)
    {
        var readmePath = Path.Combine(packageDirectory, ReadmeFileName);
        WriteTextIfDifferent(readmePath, readmeContents);
        var licensePath = Path.Combine(packageDirectory, LicenseFileName);
        WriteTextIfDifferent(licensePath, licenseContents);
        if (thirdPartyNoticesContents is not null)
        {
            var thirdPartyNoticesPath = Path.Combine(packageDirectory, ThirdPartyNoticesFileName);
            WriteTextIfDifferent(thirdPartyNoticesPath, thirdPartyNoticesContents);
        }
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
    static void WriteRuntimeJson(string packageIdentifierRuntimeJson, List<PackageInfo> infos, string filePath)
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
                  "{{packageIdentifierRuntimeJson}}": {
                    "{{info.Name}}": "{{info.Version}}"
                  }
                },
            """);
        }
        // TODO: Add remaining runtimes with base version just in case for future
        sb.Append(footer);
        WriteTextIfDifferent(filePath, sb.ToString());
    }

    static void WriteTextIfDifferent(string filePath, string content)
    {
        var contentSame = File.Exists(filePath) && File.ReadAllText(filePath) == content;
        if (!contentSame)
        {
            File.WriteAllText(filePath, content);
        }
    }
}
