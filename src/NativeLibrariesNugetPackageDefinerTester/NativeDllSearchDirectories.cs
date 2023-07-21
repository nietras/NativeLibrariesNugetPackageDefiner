﻿using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace NativeLibrariesNugetPackageDefinerTester;

#pragma warning disable CA1060 // Move pinvokes to native methods class
static class NativeDllSearchDirectories
#pragma warning restore CA1060 // Move pinvokes to native methods class
{
    // https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-setdefaultdlldirectories?redirectedfrom=MSDN
    const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
    // https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/default-probing
    const string NATIVE_DLL_SEARCH_DIRECTORIES = nameof(NATIVE_DLL_SEARCH_DIRECTORIES);
    static readonly char Separator = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';

    public static void AddDllDirectories(Action<string> log, params string[] extraSearchPaths)
    {
        // https://github.com/dotnet/fsharp/issues/10136#issuecomment-695108882

        var result = SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
        log($"{nameof(SetDefaultDllDirectories)}: {result}");

        var nativeDllSearchDirectories = (string?)AppDomain.CurrentDomain
            .GetData(NATIVE_DLL_SEARCH_DIRECTORIES) ?? string.Empty;
        // Add extra search paths if needed
        if (extraSearchPaths.Length > 0)
        {
            var addNextSeparator = nativeDllSearchDirectories.Length == 0
                || nativeDllSearchDirectories.EndsWith(Separator);
            var nextSeparator = addNextSeparator ? "" : Separator.ToString();

            nativeDllSearchDirectories += nextSeparator + string.Join(Separator, extraSearchPaths);

            AppDomain.CurrentDomain.SetData(NATIVE_DLL_SEARCH_DIRECTORIES, nativeDllSearchDirectories);
        }
        log($"{nativeDllSearchDirectories}");

        var nativeDllDirs = nativeDllSearchDirectories.Split(Separator,
            StringSplitOptions.RemoveEmptyEntries);

        // AddDllDirectory for each directory in NATIVE_DLL_SEARCH_DIRECTORIES
        // that is a sub-directory of current directory.
        var currentDirectoryInfo = new DirectoryInfo(Environment.CurrentDirectory);
        foreach (var nativeDllDir in nativeDllDirs)
        {
            var nativeDllDirInfo = new DirectoryInfo(nativeDllDir);
            if (nativeDllDirInfo.FullName.StartsWith(currentDirectoryInfo.FullName) &&
                Directory.Exists(nativeDllDirInfo.FullName))
            {
                // AddDllDirectory works if SetDefaultDllDirectories is called first
                var cookie = AddDllDirectory(nativeDllDir);
                var maybeLastErrorMessage = $"{(cookie == 0 ? new Win32Exception().Message : string.Empty)}";
                log($"{nameof(AddDllDirectory)} '{nativeDllDir}' {cookie} 'maybeLastErrorMessage'");
            }
        }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern bool SetDefaultDllDirectories(uint directoryFlags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    static extern int AddDllDirectory(string newDirectory);
}
