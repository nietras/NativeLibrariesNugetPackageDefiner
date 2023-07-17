
Runtime identifier (RID).

## TorchSharp/libtorch Walkthrough
* Create simple console application.
  ```
  dotnet new console
  ```
* Add package reference to `libtorch-cuda-11.7-win-x64` native libraries package.
  ```xml
  <Project Sdk="Microsoft.NET.Sdk">
  
    <PropertyGroup>
      <OutputType>Exe</OutputType>
      <TargetFramework>net7.0</TargetFramework>
      <ImplicitUsings>enable</ImplicitUsings>
      <Nullable>enable</Nullable>
    </PropertyGroup>
  
    <ItemGroup>
      <PackageReference Include="libtorch-cuda-11.7-win-x64" Version="2.0.1.1" />
    </ItemGroup>
    
  </Project>
  ```
* Run `dotnet restore -verbosity:detailed > restore.txt` on project. Verbosity
  to be able to check what happens. Nothing of worth here.
* Look `.nuget` package cache to see what is downloaded:
  ```
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part1"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part2"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part4"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part5-fragment1"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part5-primary"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part7"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part8"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part9-fragment1"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part9-fragment2"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part9-primary"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part10"
  "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part11"
  ```
* The main package has a `nuspec` file with:
  ```xml
  <?xml version="1.0" encoding="utf-8"?>
  <package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
      <metadata>
      <id>libtorch-cuda-11.7-win-x64</id>
      <version>2.0.1.1</version>
      <authors>PyTorch contributors,TorchSharp contributors</authors>
      <requireLicenseAcceptance>true</requireLicenseAcceptance>
      <license type="expression">MIT</license>
      <licenseUrl>https://licenses.nuget.org/MIT</licenseUrl>
      <projectUrl>https://github.com/dotnet/TorchSharp</projectUrl>
      <description>TorchSharp makes PyTorch available for .NET users. libtorch-cuda-11.7-win-x64 contains components of the PyTorch LibTorch library version 2.0.1 redistributed as a NuGet package with added support for TorchSharp.</description>
      <copyright>Copyright PyTorch contributors</copyright>
      <tags>TorchSharp LibTorch PyTorch Torch DL DNN Deep ML Machine Learning Neural Network</tags>
      <repository url="https://github.com/dotnet/" />
      <dependencies>
          <group targetFramework=".NETStandard2.0">
          <dependency id="libtorch-cuda-11.7-win-x64-part10" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part11" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part1" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part2" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part3-fragment1" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part3-primary" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part4" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part5-fragment1" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part5-primary" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part6" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part7" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part8" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part9-fragment1" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part9-fragment2" version="2.0.1.1" exclude="Build,Analyzers" />
          <dependency id="libtorch-cuda-11.7-win-x64-part9-primary" version="2.0.1.1" exclude="Build,Analyzers" />
          </group>
      </dependencies>
      </metadata>
  </package>
  ```
  Note how some of these have `-primary` and `-fragment` in their names. These
  are nuget packages with files that had to be split in order to be put on
  nuget.org.
* Run `dotnet build -verbosity:detailed > build.txt` on project.
  ```
   1:7>Target "_CopyFilesMarkedCopyLocal" in file "C:\Program Files\dotnet\sdk\7.0.400-preview.23274.1\Microsoft.Common.CurrentVersion.targets" from project "\Tester.csproj" (target "CopyFilesToOutputDirectory" depends on it):
       Using "Copy" task from assembly "Microsoft.Build.Tasks.Core, Version=15.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a".
       Task "Copy"
         Creating directory "bin\Debug\net7.0\runtimes\win-x64\native".
         Creating directory "bin\Debug\net7.0\runtimes\win-x64\native".
         Creating directory "bin\Debug\net7.0\runtimes\win-x64\native".
         Creating directory "bin\Debug\net7.0\runtimes\win-x64\native".
         Creating directory "bin\Debug\net7.0\runtimes\win-x64\native".
         Creating directory "bin\Debug\net7.0\runtimes\win-x64\native".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part1\2.0.1.1\runtimes\win-x64\native\c10.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\c10.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part1\2.0.1.1\runtimes\win-x64\native\cudart64_110.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cudart64_110.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part1\2.0.1.1\runtimes\win-x64\native\asmjit.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\asmjit.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part1\2.0.1.1\runtimes\win-x64\native\caffe2_nvrtc.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\caffe2_nvrtc.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part1\2.0.1.1\runtimes\win-x64\native\cublas64_11.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cublas64_11.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part1\2.0.1.1\runtimes\win-x64\native\c10_cuda.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\c10_cuda.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part10\2.0.1.1\runtimes\win-x64\native\torch_cpu.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\torch_cpu.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part2\2.0.1.1\runtimes\win-x64\native\cudnn_adv_infer64_8.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cudnn_adv_infer64_8.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part11\2.0.1.1\runtimes\win-x64\native\cublasLt64_11.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cublasLt64_11.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part2\2.0.1.1\runtimes\win-x64\native\cudnn_adv_train64_8.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cudnn_adv_train64_8.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part2\2.0.1.1\runtimes\win-x64\native\cudnn_ops_infer64_8.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cudnn_ops_infer64_8.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part4\2.0.1.1\runtimes\win-x64\native\cudnn64_8.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cudnn64_8.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part4\2.0.1.1\runtimes\win-x64\native\cudnn_cnn_train64_8.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cudnn_cnn_train64_8.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part4\2.0.1.1\runtimes\win-x64\native\cudnn_ops_train64_8.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cudnn_ops_train64_8.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part4\2.0.1.1\runtimes\win-x64\native\cufftw64_10.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cufftw64_10.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part4\2.0.1.1\runtimes\win-x64\native\curand64_10.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\curand64_10.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part5-primary\2.0.1.1\runtimes\win-x64\native\cufft64_10.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cufft64_10.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part5-primary\2.0.1.1\runtimes\win-x64\native\cufft64_10.dll.sha" to "\bin\Debug\net7.0\runtimes\win-x64\native\cufft64_10.dll.sha".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\cupti64_2022.2.0.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cupti64_2022.2.0.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\cusolverMg64_11.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cusolverMg64_11.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\fbgemm.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\fbgemm.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\fbjni.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\fbjni.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\libiomp5md.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\libiomp5md.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\libiompstubs5md.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\libiompstubs5md.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\nvToolsExt64_1.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\nvToolsExt64_1.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\nvfuser_codegen.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\nvfuser_codegen.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\nvrtc-builtins64_117.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\nvrtc-builtins64_117.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\nvrtc64_112_0.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\nvrtc64_112_0.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\pytorch_jni.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\pytorch_jni.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\torch.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\torch.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\torch_global_deps.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\torch_global_deps.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\uv.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\uv.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part6\2.0.1.1\runtimes\win-x64\native\zlibwapi.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\zlibwapi.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part7\2.0.1.1\runtimes\win-x64\native\cusolver64_11.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cusolver64_11.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part8\2.0.1.1\runtimes\win-x64\native\cusparse64_11.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\cusparse64_11.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part9-primary\2.0.1.1\runtimes\win-x64\native\torch_cuda.dll" to "\bin\Debug\net7.0\runtimes\win-x64\native\torch_cuda.dll".
         Copying file from "C:\Users\<USERNAME>\.nuget\packages\libtorch-cuda-11.7-win-x64-part9-primary\2.0.1.1\runtimes\win-x64\native\torch_cuda.dll.sha" to "\bin\Debug\net7.0\runtimes\win-x64\native\torch_cuda.dll.sha".
       Done executing task "Copy".
  ```
 * Use `tree /F` to see the files in the `bin` output, which shows all the
   native libraries related to `libtorch` for `win-x64` (and others). Note how
   some files have an accompanying `.sha` file. Those are files that had to be
   split into fragments in order to get the nuget package for these under the
   250 MiB limit. These `dll`s amount to about 3 GBs. In particular,
   `torch_cuda.dll` is ~990 MB alone.
   ```
   ├───bin
   │   └───Debug
   │       └───net7.0
   │           │
   │           └───runtimes
   │               └───win-x64
   │                   └───native
   │                           asmjit.dll
   │                           c10.dll
   │                           c10_cuda.dll
   │                           caffe2_nvrtc.dll
   │                           cublas64_11.dll
   │                           cublasLt64_11.dll
   │                           cudart64_110.dll
   │                           cudnn64_8.dll
   │                           cudnn_adv_infer64_8.dll
   │                           cudnn_adv_train64_8.dll
   │                           cudnn_cnn_train64_8.dll
   │                           cudnn_ops_infer64_8.dll
   │                           cudnn_ops_train64_8.dll
   │                           cufft64_10.dll
   │                           cufft64_10.dll.sha
   │                           cufftw64_10.dll
   │                           cupti64_2022.2.0.dll
   │                           curand64_10.dll
   │                           cusolver64_11.dll
   │                           cusolverMg64_11.dll
   │                           cusparse64_11.dll
   │                           fbgemm.dll
   │                           fbjni.dll
   │                           libiomp5md.dll
   │                           libiompstubs5md.dll
   │                           nvfuser_codegen.dll
   │                           nvrtc-builtins64_117.dll
   │                           nvrtc64_112_0.dll
   │                           nvToolsExt64_1.dll
   │                           pytorch_jni.dll
   │                           torch.dll
   │                           torch_cpu.dll
   │                           torch_cuda.dll
   │                           torch_cuda.dll.sha
   │                           torch_global_deps.dll
   │                           uv.dll
   │                           zlibwapi.dll
   ```
 * Add package reference to `TorchSharp` and build and you get. This now also
   has `*LibTorchSharp*` for many different runtimes.
   ```
   ├───bin
   │   └───Debug
   │       └───net7.0
   │           │   Google.Protobuf.dll
   │           │   ICSharpCode.SharpZipLib.dll
   │           │   Tester.deps.json
   │           │   Tester.dll
   │           │   Tester.exe
   │           │   Tester.pdb
   │           │   Tester.runtimeconfig.json
   │           │   SkiaSharp.dll
   │           │   TorchSharp.dll
   │           │
   │           └───runtimes
   │               ├───linux-x64
   │               │   └───native
   │               │           libLibTorchSharp.so
   │               │
   │               ├───osx
   │               │   └───native
   │               │           libSkiaSharp.dylib
   │               │
   │               ├───osx-x64
   │               │   └───native
   │               │           libLibTorchSharp.dylib
   │               │
   │               ├───win-arm64
   │               │   └───native
   │               │           libSkiaSharp.dll
   │               │
   │               ├───win-x64
   │               │   └───native
   │               │           asmjit.dll
   │               │           c10.dll
   │               │           c10_cuda.dll
   │               │           caffe2_nvrtc.dll
   │               │           cublas64_11.dll
   │               │           cublasLt64_11.dll
   │               │           cudart64_110.dll
   │               │           cudnn64_8.dll
   │               │           cudnn_adv_infer64_8.dll
   │               │           cudnn_adv_train64_8.dll
   │               │           cudnn_cnn_infer64_8.dll
   │               │           cudnn_cnn_infer64_8.dll.sha
   │               │           cudnn_cnn_train64_8.dll
   │               │           cudnn_ops_infer64_8.dll
   │               │           cudnn_ops_train64_8.dll
   │               │           cufft64_10.dll
   │               │           cufft64_10.dll.sha
   │               │           cufftw64_10.dll
   │               │           cupti64_2022.2.0.dll
   │               │           curand64_10.dll
   │               │           cusolver64_11.dll
   │               │           cusolverMg64_11.dll
   │               │           cusparse64_11.dll
   │               │           fbgemm.dll
   │               │           fbjni.dll
   │               │           libiomp5md.dll
   │               │           libiompstubs5md.dll
   │               │           libSkiaSharp.dll
   │               │           LibTorchSharp.dll
   │               │           nvfuser_codegen.dll
   │               │           nvrtc-builtins64_117.dll
   │               │           nvrtc64_112_0.dll
   │               │           nvToolsExt64_1.dll
   │               │           pytorch_jni.dll
   │               │           torch.dll
   │               │           torch_cpu.dll
   │               │           torch_cuda.dll
   │               │           torch_cuda.dll.sha
   │               │           torch_global_deps.dll
   │               │           uv.dll
   │               │           zlibwapi.dll
   │               │
   │               └───win-x86
   │                   └───native
   │                           libSkiaSharp.dll
   ```

In conclusion, TorchSharp requires manually depending on a specific runtime
nuget package. It is not handled automatically via `runtime.json` or similar.
This nuget packages depends on several other packages of which some are nuget
packages where the primary/fragment trick is used to split the package. This
shows how to host large native library files on nuget. It does, however, not
show how to automatically depend on the correct runtime package. To look at that
let's look at `libclang` that's used in `ClangSharp`.

---

## `ClangSharp`/`libclang` Walkthrough
Create simple console application in for example a `Tester` directory.
```
dotnet new console
```
Add package reference to `ClangSharp` package so `csproj` looks like:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ClangSharp" Version="16.0.0" />
  </ItemGroup>
  
</Project>
```
Run `dotnet restore -verbosity:detailed > restore.txt` on project. Verbosity set
to be able to check what happens. Nothing of worth here. Look in `.nuget`
package cache to see what is downloaded:
```
"C:\Users\<USERNAME>\.nuget\packages\clangsharp"
"C:\Users\<USERNAME>\.nuget\packages\clangsharp.interop"
"C:\Users\<USERNAME>\.nuget\packages\libclang"
"C:\Users\<USERNAME>\.nuget\packages\libclangsharp"
```
What's interesting here is no RID specific packages appear to be
downloaded (yet).
The `ClangSharp` package has a `nuspec` file with:
```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd">
  <metadata minClientVersion="4.3">
    <id>ClangSharp</id>
    <version>16.0.0</version>
    <authors>.NET Foundation and Contributors</authors>
    <requireLicenseAcceptance>true</requireLicenseAcceptance>
    <license type="expression">MIT</license>
    <licenseUrl>https://licenses.nuget.org/MIT</licenseUrl>
    <projectUrl>https://github.com/dotnet/clangsharp/</projectUrl>
    <description>ClangSharp are strongly-typed safe Clang bindings written in C# for .NET and Mono, tested on Linux and Windows.</description>
    <copyright>Copyright © .NET Foundation and Contributors</copyright>
    <repository type="git" url="https://github.com/dotnet/clangsharp/" commit="1c5588c84a5d22d2ddab41dbf7854667bf722332" />
    <dependencies>
      <group targetFramework="net6.0">
        <dependency id="ClangSharp.Interop" version="16.0.0" exclude="Build,Analyzers" />
      </group>
      <group targetFramework="net7.0">
        <dependency id="ClangSharp.Interop" version="16.0.0" exclude="Build,Analyzers" />
      </group>
      <group targetFramework=".NETStandard2.0">
        <dependency id="ClangSharp.Interop" version="16.0.0" exclude="Build,Analyzers" />
      </group>
    </dependencies>
  </metadata>
</package>
```
Jumping over the interop package and looking at `libClang` this nuspec has:
```xml
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2013/01/nuspec.xsd">
  <metadata minClientVersion="2.12">
    <id>libclang</id>
    <version>16.0.6</version>
    <authors>.NET Foundation and Contributors</authors>
    <owners>.NET Foundation and Contributors</owners>
    <requireLicenseAcceptance>true</requireLicenseAcceptance>
    <license type="expression">Apache-2.0 WITH LLVM-exception</license>
    <licenseUrl>https://licenses.nuget.org/Apache-2.0%20WITH%20LLVM-exception</licenseUrl>
    <projectUrl>https://github.com/dotnet/clangsharp</projectUrl>
    <description>Multi-platform native library for libclang.</description>
    <copyright>Copyright © LLVM Project</copyright>
    <repository type="git" url="https://github.com/llvm/llvm-project" branch="llvmorg-16.0.6" />
    <dependencies>
      <group targetFramework=".NETStandard2.0" />
    </dependencies>
  </metadata>
</package>
```
That's interesting given it has no dependencies and contains no libraries:
```
"C:\Users\<USERNAME>\.nuget\packages\libclang\16.0.6\.nupkg.metadata"
"C:\Users\<USERNAME>\.nuget\packages\libclang\16.0.6\.signature.p7s"
"C:\Users\<USERNAME>\.nuget\packages\libclang\16.0.6\libclang.16.0.6.nupkg"
"C:\Users\<USERNAME>\.nuget\packages\libclang\16.0.6\libclang.16.0.6.nupkg.sha512"
"C:\Users\<USERNAME>\.nuget\packages\libclang\16.0.6\libclang.nuspec"
"C:\Users\<USERNAME>\.nuget\packages\libclang\16.0.6\LICENSE.TXT"
"C:\Users\<USERNAME>\.nuget\packages\libclang\16.0.6\runtime.json"
```
But what's in the `runtime.json` file:
```json
{
  "runtimes": {
    "linux-arm64": {
      "libclang": {
        "libclang.runtime.linux-arm64": "16.0.6"
      }
    },
    "linux-x64": {
      "libclang": {
        "libclang.runtime.linux-x64": "16.0.6"
      }
    },
    "osx-arm64": {
      "libclang": {
        "libclang.runtime.osx-arm64": "16.0.6"
      }
    },
    "osx-x64": {
      "libclang": {
        "libclang.runtime.osx-x64": "16.0.6"
      }
    },
    "win-arm64": {
      "libclang": {
        "libclang.runtime.win-arm64": "16.0.6"
      }
    },
    "win-x64": {
      "libclang": {
        "libclang.runtime.win-x64": "16.0.6"
      }
    },
    "win-x86": {
      "libclang": {
        "libclang.runtime.win-x86": "16.0.6"
      }
    }
  }
}
```
Ah, that appears to map RIDs to runtime specific packages. But none were
downloaded, so what happens when we build the project. Run `dotnet build
-verbosity:detailed > build.txt` on project. Examining the build output and the
`.nuget` cache none of those runtime specific packages appear to be downloaded
(yet). Let's try running the project with some dummy code in `Program.cs`.
```csharp
using ClangSharp.Interop;

using var index = CXIndex.Create();
```
It runs, but still no runtime specific packages downloaded nor any native
libraries in build output. Let's try a more involved example copied from a unit
test in `ClangSharp`.
```csharp
// https://github.com/dotnet/ClangSharp/blob/main/tests/ClangSharp.UnitTests/CXTranslationUnitTest.cs
using ClangSharp.Interop;
using static ClangSharp.Interop.CXTranslationUnit_Flags;

var name = "basic";
var dir = Path.GetRandomFileName();
_ = Directory.CreateDirectory(dir);

try
{
    // Create a file with the right name
    var file = new FileInfo(Path.Combine(dir, name + ".c"));
    File.WriteAllText(file.FullName, "int main() { return 0; }");

    using var index = CXIndex.Create();
    using var translationUnit = CXTranslationUnit.Parse(
        index, file.FullName, Array.Empty<string>(),
        Array.Empty<CXUnsavedFile>(), CXTranslationUnit_None);
    var clangFile = translationUnit.GetFile(file.FullName);
}
finally
{
    Directory.Delete(dir, true);
}
```
This runs fine. But still no runtime specific packages downloaded nor any
native libraries in build output. Let's trying running the code in Visual
Studio with native debugging enabled. That is add launch settings with
`"nativeDebugging": true`. This is just a quick way to look at which native
libraries are loaded and from where. Many ways of doing that, just using
Visual Studio since quick and easy. In the **Debug** window one can see:
```
(Win32): Loaded '\bin\Debug\net7.0\ClangSharp.Interop.dll'. 
(CoreCLR: clrhost): Loaded '\bin\Debug\net7.0\ClangSharp.Interop.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
(Win32): Loaded 'C:\Program Files\LLVM\bin\libclang.dll'. Module was built without symbols.
```
Ah, turns out I have LLVM with clang installed 🤷‍ So this must be in
environment variable `PATH`. Which it turns out it is `C:\Program
Files\LLVM\bin`. Let's try removing that, and restart all consoles, applications
in use.

Running the example program again will then fail with exception:
```
System.DllNotFoundException: 'Unable to load DLL 'libclang' or one of its dependencies: 
The specified module could not be found. (0x8007007E)'
```
Hmm, so the `libclang` native library is not available and the package is not
downloaded automatically? How does `runtime.json` then work?
Let's try running the application with a runtime identifier defined:
```
dotnet run -r win-x64 > run.txt
```
This takes a while, and only output is:
```
C:\Program Files\dotnet\sdk\7.0.400-preview.23274.1\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.targets(1142,5): 
  warning NETSDK1179: One of '--self-contained' or '--no-self-contained' options are required when '--runtime' is used. 
  [Tester.csproj]
Tester\4lzfbeoi.214\basic.c
```
but the program runs fine. Looking in `.nuget` and we can see the runtime
specific packages have actually been downloaded now.
```
"C:\Users\<USERNAME>\.nuget\packages\libclangsharp.runtime.win-x64"
"C:\Users\<USERNAME>\.nuget\packages\libclang.runtime.win-x64"
```
so what this means is we cannot actually run and define the application without
specifying a runtime identifier? That's seems problematic if we want to use this
as framework dependent AnyCPU application... in fact if we run the application
from Visual Studio again it will fail with the same exception as before. 

Use `tree /F` to see the files in the `bin` output, which shows all the native
libraries related to `libclang` for `win-x64` (and others). 
```
├───bin
│   └───Debug
│       └───net7.0
│           │   ClangSharp.dll
│           │   ClangSharp.Interop.dll
│           │   Tester.deps.json
│           │   Tester.dll
│           │   Tester.exe
│           │   Tester.pdb
│           │   Tester.runtimeconfig.json
│           │
│           ├───egfakait.om3
│           │       basic.c
│           │
│           └───win-x64
│                   ClangSharp.dll
│                   ClangSharp.Interop.dll
│                   clretwrc.dll
│                   clrgc.dll
│                   clrjit.dll
│                   coreclr.dll
│                   createdump.exe
│                   hostfxr.dll
│                   hostpolicy.dll
│                   libclang.dll
│                   libClangSharp.dll
│                   Microsoft.CSharp.dll
│                   Microsoft.DiaSymReader.Native.amd64.dll
│                   Microsoft.VisualBasic.Core.dll
│                   Microsoft.VisualBasic.dll
│                   Microsoft.Win32.Primitives.dll
│                   Microsoft.Win32.Registry.dll
│                   mscordaccore.dll
│                   mscordaccore_amd64_amd64_7.0.523.17405.dll
│                   mscordbi.dll
│                   mscorlib.dll
│                   mscorrc.dll
│                   msquic.dll
│                   Tester.deps.json
│                   Tester.dll
│                   Tester.exe
│                   Tester.pdb
│                   Tester.runtimeconfig.json
│                   netstandard.dll
                    // Almost all System.*dlls follow here
│                   System.*.dll
```
Note how this has an `exe` under the specific runtime folder and all the dlls
next to it.

As far as I can tell this means the `runtime.json` way of mapping runtime
identifier specific packages only works if you define a **hard-coded** specific
runtime identifier in the program you want to run. Which is incredibly annoying
if you want to build and deploy runtime agnostic applications. E.g. if we wanted
to deploy a `win-x86` + `win-x64` single exe. How is that supposed to work then?
Am I getting this wrong?

Let's try a hack. Adding the RID specific package to the project. That is add
`<PackageReference Include="libclang.runtime.win-x64" Version="16.0.0" />` to
the project. Run it from VS and then it now runs fine. Right, so in some ways
this works fine if we add the RID specific packages explicitly.

Still how does this work with regards to testing and if you use MSTest for both
x86 and x64 testing? Let's add a unit test project and reference the tester
console project, and copy code from above unit test in `Program.cs` into this
project. Now if we run the unit test with **Processor Architecture for AnyCPU
Projects** set to `Auto`. If we change this to `x86` and it will fail with
the same exception as before:
```
System.DllNotFoundException: Unable to load DLL 'libclang' or one of its dependencies: 
The specified module could not be found. (0x8007007E)
```
Interestingly, in the output we will get:
```
*****IMPORTANT*****
Failed to resolve libclang.
If you are running as a dotnet tool, you may need to manually copy the appropriate DLLs 
from NuGet due to limitations in the dotnet tool support. 
Please see https://github.com/dotnet/clangsharp for more details.
*****IMPORTANT*****
```
Note that the RID is `win10-x86` in this case if logged e.g. with
`log(RuntimeInformation.RuntimeIdentifier);`. If we select `x64` it is
`win10-x64` and the test succeeds, but only because we added the RID specific
`libclang.runtime.win-x64` package to the project.

In
[https://github.com/dotnet/ClangSharp/issues/118#issuecomment-598305888](https://github.com/dotnet/ClangSharp/issues/118#issuecomment-598305888)
this issue is expanded upon with the comment by Tanner Gooding:

> The simple fix for now is to add `<RuntimeIdentifier Condition="'$(RuntimeIdentifier)' == '' AND '$(PackAsTool)' != 'true'">$(NETCoreSdkRuntimeIdentifier)</RuntimeIdentifier>` 
> to your project
> (under a PropertyGroup), unfortunately because of the way NuGet restore works,
> we can't just add this to a build/*.targets in the ClangSharp nuget package.
> 
> The issue is essentially that libClang and libClangSharp just contain a
> runtime.json file which point to the real packages. This was done to avoid
> users needing to download hundreds of megabytes just to consume ClangSharp
> (when they only need one of the native binaries most often). You can see some
> more details on the sizes here: #46 (comment), noting that that is the size of
> the compressed NuGet.
>
> I had thought this was working for dev scenarios where the RID wasn't
> specified, but it apparently isn't. I'll log an issue on NuGet to see if this
> is something that can be improved.

I wonder whether this actually works for the case of switching processor
architecture in VS or similar? Let's try adding it to the unit tests project and
remove the RID specific package from the console project. Hence we have console
project:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ClangSharp" Version="16.0.0" />
  </ItemGroup>
  
</Project>
```
and unit test project:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <IsPackable>false</IsPackable>

    <RuntimeIdentifier Condition="'$(RuntimeIdentifier)' == '' AND '$(PackAsTool)' != 'true'">$(NETCoreSdkRuntimeIdentifier)</RuntimeIdentifier>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.3.2" />
    <PackageReference Include="MSTest.TestAdapter" Version="2.2.10" />
    <PackageReference Include="MSTest.TestFramework" Version="2.2.10" />
    <PackageReference Include="coverlet.collector" Version="3.1.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Tester\Tester.csproj" />
  </ItemGroup>

</Project>
```
First time you then try to build this you will get a well-known error:
```
Assets file 'TesterUnitTests\obj\project.assets.json' doesn't have a target for 'net7.0/win-x64'. 
Ensure that restore has run and that you have included 'net7.0' in the TargetFrameworks for your project. 
You may also need to include 'win-x64' in your project's RuntimeIdentifiers.
```
So restore and build again. Let's try running `x86` unit tests in VS. This
succeeds but the RID is actually now `win10-x64`, so we can now no longer run or
debug `x86` tests from Visual Studio?

Let's first try to define test running via a script `test-x86-x64.ps1`:
```powershell
#!/usr/bin/env pwsh
Write-Host "Testing Debug X86"
dotnet test --nologo -c Debug -- RunConfiguration.TargetPlatform=x86
Write-Host "Testing Release X86"
dotnet test --nologo -c Release -- RunConfiguration.TargetPlatform=x86
Write-Host "Testing Debug X64"
dotnet test --nologo -c Debug -- RunConfiguration.TargetPlatform=x64
Write-Host "Testing Release X64"
dotnet test --nologo -c Release -- RunConfiguration.TargetPlatform=x64
```
For `x86` this will then fail with:
```
Test run detected DLL(s) which would use different framework and platform versions. Following DLL(s) do not match current settings, which are .NETCoreApp,Version=v7.0 framework and X86 platform.
TesterUnitTests.dll would use Framework .NETCoreApp,Version=v7.0 and Platform X64.
```
again this isn't great. We need to be able to run both x64 and x86 without
having to go through hoops.

Perhaps if we add both `win-x64` and `win-x86` to a `RuntimeIdentifiers`
property instead? So change
```xml
    <RuntimeIdentifier Condition="'$(RuntimeIdentifier)' == '' AND '$(PackAsTool)' != 'true'">$(NETCoreSdkRuntimeIdentifier)</RuntimeIdentifier>
```
```xml
    <RuntimeIdentifiers>win-x64;win-x86</RuntimeIdentifiers>
```
then run `test-x86-x64.ps1`. Now everything fails with the same exception:
```
System.DllNotFoundException: Unable to load DLL 'libclang' or one of its dependencies: 
The specified module could not be found. (0x8007007E)
```
According to
[https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#runtimeidentifiers](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#runtimeidentifiers)
I should have defined the RIDs correctly. An example from there is:
```xml
<PropertyGroup>
  <RuntimeIdentifiers>win10-x64;osx.10.11-x64;ubuntu.16.04-x64</RuntimeIdentifiers>
</PropertyGroup>
```
Okay, perhaps running tests then need to be done differently and not with
the `RunConfiguration.TargetPlatform` property? Let's try to run the tests
with `--runtime` instead in a new script `test-x86-x64-rid.ps1`:
```powershell
#!/usr/bin/env pwsh
Write-Host "Testing Debug win-x86"
dotnet test --nologo -c Debug --runtime win-x86
Write-Host "Testing Release win-x86"
dotnet test --nologo -c Release --runtime win-x86
Write-Host "Testing Debug win-x64" 
dotnet test --nologo -c Debug --runtime win-x64
Write-Host "Testing Release win-x64"
dotnet test --nologo -c Release --runtime win-x64
```
Then the tests succeed, albeit with the annoying warnings below.
```
C:\Program Files\dotnet\sdk\7.0.400-preview.23274.1\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.targets(1142,5): 
warning NETSDK1179: One of '--self-contained' or '--no-self-contained' options are required when '--runtime' is used. [TesterUnitTests\TesterUnitTests.csproj]
C:\Program Files\dotnet\sdk\7.0.400-preview.23274.1\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.targets(1142,5): 
warning NETSDK1179: One of '--self-contained' or '--no-self-contained' options are required when '--runtime' is used. [Tester.csproj]
```
why do I need to specify whether to be self-contained or not when I am just
running tests? I am not publishing?

And are the tests really running x86 as expected? To test this I add two simple test:
```csharp
    [TestMethod]
    public void X86() => Assert.AreEqual("win10-x86", RuntimeInformation.RuntimeIdentifier);
    [TestMethod]
    public void X64() => Assert.AreEqual("win10-x64", RuntimeInformation.RuntimeIdentifier);
```
and run the tests again. On `win-x86` the `X64` test fails as expected:
```
Assert.AreEqual failed. Expected:<win10-x64>. Actual:<win10-x86>.
```
and vice versa on `win-x64`:
```
Assert.AreEqual failed. Expected:<win10-x86>. Actual:<win10-x64>.
```
so at least that works as expected.

Let's try running these tests from Visual Studio again. First, by setting
processor architecture to `x86`. All tests except `x86` fail, so this does switch the
runtime identifier to `win10-x86`, but it does not fix the `libclang` problem.
```
System.DllNotFoundException: Unable to load DLL 'libclang' or one of its dependencies: 
The specified module could not be found. (0x8007007E)
```
so even though RIDs are now specified this doesn't work when running tests from
VS? Switching to `x64` in VS and then only `X64` test passes, and still the
`libclang` dll cannot be found, so now this doesn't work either. The difference
apparently being there is now multiple RIDs, not just one.

Only way I think this can then be resolved is to actually explicitly add those
RID specific runtime packages after all then so console project looks like:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net7.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="libtorch-cuda-11.7-win-x64" Version="2.0.1.1" />-->
    <PackageReference Include="ClangSharp" Version="16.0.0" />
    <PackageReference Include="libclang.runtime.win-x64" Version="16.0.6" />
    <PackageReference Include="libclang.runtime.win-x86" Version="16.0.6" />
  </ItemGroup>
  
</Project>
```
Re-running the unit tests and now `libclang` can be loaded and that test
succeeds. Let's try command line too and it's the same.

So after all this, it seems like the `runtime.json` way of packaging native
libraries has it's set of challenges, you basically end up having explicitly add
the RID specific packages anyway if you target multiple RIDs. In the process you
then end up implicitly forcing the Any CPU build to no longer be frame dependent
but self-contained? This is all very confusing and hard to understand and not
the least convey to other developers.

TODO: Address issue if using multiple `RuntimeIdentifiers` line `win-x64;win-x86`,
then nothing is copied to output folder using the `runtime.json` approach.

## Links
 * "[Feature] Increase the package size limit on NuGet.org from 250 MB"
   https://github.com/NuGet/NuGetGallery/issues/9473
   This features a discussion on how to split a nuget package and links.
 * TorchSharp - package uses primary/fragment trick
   https://www.nuget.org/packages/libtorch-cuda-11.7-win-x64/
 * SciSharp.TensorFlow.Redist - adopts same trick
   https://www.nuget.org/packages/SciSharp.TensorFlow.Redist-Linux-GPU#dependencies-body-tab
 * "NuGet 3: The Runtime ID Graph" - discusses `runtime.json` - not sure how to get this to work
   https://natemcmaster.com/blog/2016/05/19/nuget3-rid-graph/
 * "Improve handling of native packages (Support RID specific dependencies)" - shows how libclang uses this trick
   https://github.com/NuGet/Home/issues/10571
   https://www.nuget.org/packages/libclang
   https://www.nuget.org/packages/libclang.runtime.win-x64/
 * "MSBuild inline tasks with RoslynCodeTaskFactory"
   https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild-roslyncodetaskfactory?view=vs-2022
 * "Shipping a cross-platform MSBuild task in a NuGet package"
   https://natemcmaster.com/blog/2017/07/05/msbuild-task-in-nuget/
 * "Architecture-specific folders like runtimes/<rid>/native/ outside of NuGet packages [nativeinterop]"
   https://github.com/dotnet/sdk/issues/24708
 * "Should runtime. packages be listed in NuGet.org?"
   https://github.com/dotnet/core/issues/7568
 * "Create a nuget package"
   https://github.com/vincenzoml/SimpleITK-dotnet-quickstart/issues/1
 * "Add a way to list native assets that a project will load from the app directory (list them in deps.json)"
   https://github.com/dotnet/sdk/issues/11373
 * "Guide for packaging C# library using P/Invoke to per-architecture and/or per-platform C++ native DLLs"
   https://github.com/NuGet/Home/issues/8623
 * Mizux `dotnet-native` template git repository
   https://github.com/Mizux/dotnet-native