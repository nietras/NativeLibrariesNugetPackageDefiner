// https://github.com/dotnet/ClangSharp/blob/main/tests/ClangSharp.UnitTests/CXTranslationUnitTest.cs
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

Action<string> log = t => { Trace.WriteLine(t); Console.WriteLine(t); };

log(RuntimeInformation.RuntimeIdentifier);

using var options = new SessionOptions();
if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
{
    options.AppendExecutionProvider_CUDA();
    options.AppendExecutionProvider_Tensorrt();
}
using var inference = new InferenceSession("smallsimpledense_0_1_0.onnx");

var namedOnnxValues = inference.InputMetadata.Select(
    p => NamedOnnxValue.CreateFromTensor(p.Key, new DenseTensor<float>(p.Value.Dimensions))).ToArray();

foreach (var i in inference.InputNames) { log($"Input: {i}"); };

using var output = inference.Run(namedOnnxValues);

foreach (var o in output) { log($"Output: {o.Name}"); };

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
