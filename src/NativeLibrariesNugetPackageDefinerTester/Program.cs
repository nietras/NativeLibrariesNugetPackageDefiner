﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

Action<string> log = t => { Trace.WriteLine(t); Console.WriteLine(t); };

log(RuntimeInformation.RuntimeIdentifier);
log(Environment.CurrentDirectory);


// https://docs.nvidia.com/cuda/cuda-c-programming-guide/index.html#lazy-loading
// Lazy Loading delays loading of CUDA modules and kernels from program
// initialization closer to kernels execution. If a program does not use every
// single kernel it has included, then some kernels will be loaded
// unnecessarily. This is very common, especially if you include any libraries.
// Most of the time, programs only use a small amount of kernels from libraries
// they include.
//
// Thanks to Lazy Loading, programs are able to only load kernels they are
// actually going to use, saving time on initialization. This reduces memory
// overhead, both on GPU memory and host memory
//
// Lazy Loading is enabled by setting the CUDA_MODULE_LOADING environment
// variable to LAZY.
//
// Firstly, CUDA Runtime will no longer load all modules during program
// initialization, with the exception of modules containing managed variables.
// Each module will be loaded on first usage of a variable or a kernel from that
// module. This optimization is only relevant to CUDA Runtime users, CUDA Driver
// users are unaffected. This optimization shipped in CUDA 11.8.
//
// Secondly, loading a module (cuModuleLoad*() family of functions) will not be
// loading kernels immediately, instead it will delay loading of a kernel until
// cuModuleGetFunction() is called.There are certain exceptions here, some
// kernels have to be loaded during cuModuleLoad*(), such as kernels of which
// pointers are stored in global variables. This optimization is relevant to
// both CUDA Runtime and CUDA Driver users. CUDA Runtime will only call
// cuModuleGetFunction() when a kernel is used/referenced for the first time.
// This optimization shipped in CUDA 11.7.
//
// Both of these optimizations are designed to be invisible to the user,
// assuming CUDA Programming Model is followed.
Environment.SetEnvironmentVariable("CUDA_MODULE_LOADING", "LAZY");

// https://github.com/dotnet/fsharp/issues/10136#issuecomment-695108882

// https://learn.microsoft.com/en-us/windows/win32/api/libloaderapi/nf-libloaderapi-setdefaultdlldirectories?redirectedfrom=MSDN
const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
var result = SetDefaultDllDirectories(LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
log($"{nameof(SetDefaultDllDirectories)}: {result}");
// AddDllDirectory for each directory in NATIVE_DLL_SEARCH_DIRECTORIES that is a
// sub-directory of current directory.
// https://learn.microsoft.com/en-us/dotnet/core/dependency-loading/default-probing
var nativeDllSearchDirectoriesDelimiter = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ';' : ':';
var arch = Environment.Is64BitProcess ? @"x64" : @"x86";
var ourCustomDllArchDirectory = Path.Combine(Environment.CurrentDirectory, arch);
const string NATIVE_DLL_SEARCH_DIRECTORIES = "NATIVE_DLL_SEARCH_DIRECTORIES";
var nativeDllSearchDirectories = (string?)AppDomain.CurrentDomain.GetData(NATIVE_DLL_SEARCH_DIRECTORIES) ?? string.Empty;
nativeDllSearchDirectories += ((nativeDllSearchDirectories.EndsWith(nativeDllSearchDirectoriesDelimiter) || nativeDllSearchDirectories.Length == 0)
    ? "" : nativeDllSearchDirectoriesDelimiter)
    + ourCustomDllArchDirectory;
AppDomain.CurrentDomain.SetData(NATIVE_DLL_SEARCH_DIRECTORIES, nativeDllSearchDirectories);
log($"{nativeDllSearchDirectories}");
if (nativeDllSearchDirectories is not null)
{
    var currentDirectoryInfo = new DirectoryInfo(Environment.CurrentDirectory);
    var nativeDllDirs = nativeDllSearchDirectories.Split(nativeDllSearchDirectoriesDelimiter, StringSplitOptions.RemoveEmptyEntries);
    foreach (var nativeDllDir in nativeDllDirs)
    {
        var nativeDllDirInfo = new DirectoryInfo(nativeDllDir);
        if (nativeDllDirInfo.FullName.StartsWith(currentDirectoryInfo.FullName) && Directory.Exists(nativeDllDirInfo.FullName))
        {
            // AddDllDirectory works if SetDefaultDllDirectories is called first
            var cookie = AddDllDirectory(nativeDllDir);
            log($"{nameof(AddDllDirectory)} '{nativeDllDir}' {cookie} '{(cookie == 0 ? new Win32Exception().Message : string.Empty)}'");
        }
    }
}
//var arch = Environment.Is64BitProcess ? @"x64" : @"x86";
//var runtimeRelativeDir = $"runtimes/win-{arch}/native";
//var runtimesDir = Path.Combine(Environment.CurrentDirectory, runtimeRelativeDir);
// AddDllDirectory works if SetDefaultDllDirectories is called first
//var handle = AddDllDirectory(runtimesDir);
//log($"AddDllDirectory: {handle}");
// SetDllDirectory works but this overrides previous set and only allows one directory
// https://stackoverflow.com/questions/44588618/setdlldirectory-does-not-cascade-so-dependency-dlls-cannot-be-loaded
//var setDll = SetDllDirectory(runtimesDir);
//log($"SetDllDirectory: {setDll}");
// Adding to PATH environment variable also works but is discouraged
//const string PATH = nameof(PATH);
//var pathEnvVar = Environment.GetEnvironmentVariable(PATH);
//pathEnvVar += $";{runtimesDir}";
//Environment.SetEnvironmentVariable(PATH, pathEnvVar);

// Loads fine without PATH or AddDllDirectory, but we are not in charge of loading these
//NativeLibrary.Load(@"nvinfer_builder_resource.dll", typeof(Program).Assembly, DllImportSearchPath.SafeDirectories);
//NativeLibrary.Load(@"cudnn_cnn_infer64_8.dll", typeof(Program).Assembly, DllImportSearchPath.SafeDirectories);

var modelPath = "smallsimpledense_0_1_0.onnx";

var env = OrtEnv.Instance;
//NativeApiStatus.VerifySuccess(NativeMethods.OrtCreateEnv(LogLevel.Warning, @"CSharpOnnxRuntime", out var handle));
//var bytes = File.ReadAllBytes(modelPath);
//var availableProviders = InferenceSessionFactory.FindAvailablePrioritizedExecutionProviders(
//    ExecutionProviders.DefaultPrioritizedList)
//    .ToArray();

using var options = new SessionOptions();
if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
{
    options.AppendExecutionProvider_Tensorrt();
    options.AppendExecutionProvider_CUDA();
}
using var inference = new InferenceSession(modelPath, options);

var namedOnnxValues = inference.InputMetadata.Select(
    p => NamedOnnxValue.CreateFromTensor(p.Key, new DenseTensor<float>(p.Value.Dimensions))).ToArray();

foreach (var i in inference.InputMetadata.Keys) { log($"Input: {i}"); };

using var output = inference.Run(namedOnnxValues);

foreach (var o in output) { log($"Output: {o.Name}"); };

[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
static extern int AddDllDirectory(string newDirectory);

[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
[return: MarshalAs(UnmanagedType.Bool)]
[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
static extern bool SetDllDirectory(string lpPathName);

[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
static extern bool SetDefaultDllDirectories(uint directoryFlags);

// https://github.com/dotnet/ClangSharp/blob/main/tests/ClangSharp.UnitTests/CXTranslationUnitTest.cs
//var name = "basic";
//var dir = Path.GetRandomFileName();
//_ = Directory.CreateDirectory(dir);

//try
//{
//    // Create a file with the right name
//    var file = new FileInfo(Path.Combine(dir, name + ".c"));
//    File.WriteAllText(file.FullName, "int main() { return 0; }");

//    using var index = CXIndex.Create();
//    using var translationUnit = CXTranslationUnit.Parse(
//        index, file.FullName, Array.Empty<string>(),
//        Array.Empty<CXUnsavedFile>(), CXTranslationUnit_None);
//    var clangFile = translationUnit.GetFile(file.FullName);
//    log(clangFile.ToString());
//}
//finally
//{
//    Directory.Delete(dir, true);
//}

//using TorchSharp;
//using static TorchSharp.torch.nn;

//var lin1 = Linear(1000, 100);
//var lin2 = Linear(100, 10);
//var seq = Sequential(("lin1", lin1), ("relu1", ReLU()), ("drop1", Dropout(0.1)), ("lin2", lin2)).cuda();

//var x = torch.randn(64, 1000).cuda();
//var y = torch.randn(64, 10).cuda();

//var optimizer = torch.optim.Adam(seq.parameters());

//for (int i = 0; i < 10; i++)
//{
//    var eval = seq.forward(x);
//    var output = functional.mse_loss(eval, y, Reduction.Sum);

//    optimizer.zero_grad();

//    output.backward();

//    optimizer.step();
//}
