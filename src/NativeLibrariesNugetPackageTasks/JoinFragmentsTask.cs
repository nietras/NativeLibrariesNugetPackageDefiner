using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Build.Framework;

// BAD IDEA TO USE THIS FROM A NUGET PACKAGE SO INSTEAD JUST TRYING TO USE THE
// TASK TO AUTHOR THE SEPARATE CODE FILE FOR THE MSBUILD TARGETS FILE BY
// REFERENCING THIS FILE DIRECTLY. MAKES DEV LOOP FASTER AND EASIER.
public class JoinFragmentsTask : Microsoft.Build.Utilities.Task
{
    public string FileToJoinFragmentsFor { get; set; }
    public int FragmentsCount { get; set; }
    public long ExpectedSize { get; set; }
    public string ExpectedHash { get; set; }

    public override bool Execute()
    {
        // targetsFileName should be same as package identifier
        var targetsFilePath = BuildEngine.ProjectFileOfTaskNode;
        string targetsFileName = Path.GetFileNameWithoutExtension(targetsFilePath);

        Action<string> log = t => Log.LogMessage(MessageImportance.High, $"{nameof(JoinFragmentsTask)}-[{targetsFileName}]: {t}");
        Action<string> logDetail = t => { }; //log;
        Action<string> logError = t => Log.LogError($"{nameof(JoinFragmentsTask)}-[{targetsFileName}]: {t}");
        try
        {
            var fileToJoinPath = Path.GetFullPath(FileToJoinFragmentsFor);
            if (!File.Exists(fileToJoinPath))
            { logError($"Could not find file-to-join at {fileToJoinPath}"); return false; }

            // On Linux it appears there must be a specific scope prefix for the
            // mutex to work. Completely untested.
            const string MutexGlobalPrefix = @"Global\";
            // Add a guid for good measure to ensure unique
            var mutexName = MutexGlobalPrefix + targetsFileName + "-CDC385E6-D34E-480B-AF6D-E6968E48B873";
            using var mutex = new Mutex(false, mutexName, out var createdNew);
            logDetail($"{(createdNew ? "Created" : "Opened")} named mutex '{mutexName}' " +
                      $"so only one at a time is joining fragments for '{targetsFileName}'.");
            var wait = mutex.WaitOne(60 * 1000);
            if (!wait) { logError($"Wait timed out on named mutex '{mutexName}'"); return false; }
            try
            {
                var fileToJoinInfo = new FileInfo(fileToJoinPath);
                logDetail($"Found file-to-join at {fileToJoinPath} {fileToJoinInfo.Length}");

                // If size is as expected, assume file already joined
                if (fileToJoinInfo.Length == ExpectedSize) { return true; }

                log($"Found placeholder file-to-join at '{fileToJoinPath}'");

                // file-to-join path:
                // "<NUGETCACHE>/<PACKAGE-ID-RUNTIME-SPECIFIC>\<VERSION>\runtimes\win-x64\native\<FILETOJOINFRAGMENTSFOR>"
                // Targets file path:
                // "<NUGETCACHE>/<PACKAGE-ID-RUNTIME-SPECIFIC>\<VERSION>\buildTransitive\.netstandard1.1\<PACKAGE-ID-RUNTIME-SPECIFIC>.targets"
                // Fragments are defined to be at:
                // "<NUGETCACHE>/<PACKAGE-ID-RUNTIME-SPECIFIC>.f<XX>\<VERSION>\fragments\win-x64\native\<FILETOJOINFRAGMENTSFOR>.f<XX>"

                logDetail($"Targets file name/package id '{targetsFileName}'");

                var allFragmentFiles = Enumerable.Range(0, FragmentsCount)
                    .Select(i =>
                    {
                        var fragmentSuffix = $".f{i:D2}";
                        var idIndex = fileToJoinPath.LastIndexOf(targetsFileName, StringComparison.OrdinalIgnoreCase);
                        if (idIndex < 0) { throw new InvalidDataException($"Could not find package id '{targetsFileName}' in '{fileToJoinPath}'"); }
                        var fragmentFilePath = fileToJoinPath.Insert(idIndex + targetsFileName.Length, fragmentSuffix);
                        fragmentFilePath = fragmentFilePath.Replace("runtimes", "fragments") + fragmentSuffix;
                        return fragmentFilePath;
                    })
                    .ToList();
                foreach (var fragmentFile in allFragmentFiles)
                {
                    logDetail($"Checking fragment file at '{fragmentFile}' exists {File.Exists(fragmentFile)}");
                }

                var fragmentFiles = allFragmentFiles.Where(File.Exists).ToList();

                if (fragmentFiles.Count != FragmentsCount)
                {
                    // TODO: Expand on deleting packages from package cache to start over
                    logError($"Found {fragmentFiles.Count} fragment files, expected {FragmentsCount}. {string.Join(",", fragmentFiles)}");
                    return false;
                }

                log($"Overwriting file-to-join at '{fileToJoinPath}'");
                using (var fileToJoinStream = File.Create(fileToJoinPath))
                {
                    foreach (var fragmentFile in fragmentFiles)
                    {
                        log($"Copying fragment bytes from '{fragmentFile}' to '{fileToJoinPath}'");
                        using var fragmentFileStream = File.OpenRead(fragmentFile);
                        fragmentFileStream.CopyTo(fileToJoinStream);
                    }
                }

                fileToJoinInfo = new FileInfo(fileToJoinPath);
                if (fileToJoinInfo.Length != ExpectedSize)
                {
                    logError($"Joined file size {fileToJoinInfo.Length} not equal to expected {ExpectedSize} '{fileToJoinPath}'");
                    return false;
                }

                log($"Checking file hash");
                var hash = ComputeHashSHA256(fileToJoinPath);
                if (hash != ExpectedHash)
                {
                    logError($"Joined file hash {hash} not equal to expected {ExpectedHash} '{fileToJoinPath}'");
                    return false;
                }
                log($"File hash {hash} matches {ExpectedHash} for '{fileToJoinPath}'");

                foreach (var fragmentFile in fragmentFiles)
                {
                    log($"Deleting fragment file '{fragmentFile}' to reduce disk usage after join");
                    File.Delete(fragmentFile);
                }
            }
            finally
            {
                mutex.ReleaseMutex();
                logDetail($"Released named mutex '{mutexName}'");
            }

        }
        catch (Exception ex)
        {
            logError(ex.ToString());
            logError(ex.StackTrace);
        }
        return true;
    }

    static string ComputeHashSHA256(string path)
    {
        using var stream = File.OpenRead(path);
        using var hash = System.Security.Cryptography.SHA256.Create();
        return BitConverter.ToString(hash.ComputeHash(stream));
    }
}
