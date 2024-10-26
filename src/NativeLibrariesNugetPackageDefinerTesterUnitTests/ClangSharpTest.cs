//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using ClangSharp.Interop;
//using static ClangSharp.Interop.CXTranslationUnit_Flags;

//namespace NativeLibrariesNugetPackageDefinerTesterUnitTests;

//[TestClass]
//public class ClangSharpTest
//{
//    [TestMethod]
//    public void ClangSharpTest_TranslationUnit()
//    {
//        Action<string> log = t => { Trace.WriteLine(t); Console.WriteLine(t); };

//        log(RuntimeInformation.RuntimeIdentifier);
//        //string runtimeIdentifier = RuntimeInformation.RuntimeIdentifier == "win10-x64" ? "win-x64" : "win-x86";

//        var name = "basic";
//        var dir = Path.GetRandomFileName();
//        _ = Directory.CreateDirectory(dir);

//        try
//        {
//            // Create a file with the right name
//            var file = new FileInfo(Path.Combine(dir, name + ".c"));
//            File.WriteAllText(file.FullName, "int main() { return 0; }");

//            using var index = CXIndex.Create();
//            using var translationUnit = CXTranslationUnit.Parse(
//                index, file.FullName, Array.Empty<string>(),
//                Array.Empty<CXUnsavedFile>(), CXTranslationUnit_None);
//            var clangFile = translationUnit.GetFile(file.FullName);
//            log(clangFile.ToString());
//        }
//        finally
//        {
//            Directory.Delete(dir, true);
//        }

//    }
//    [TestMethod]
//    public void X86() => Assert.AreEqual("win10-x86", RuntimeInformation.RuntimeIdentifier);
//    [TestMethod]
//    public void X64() => Assert.AreEqual("win10-x64", RuntimeInformation.RuntimeIdentifier);
//}