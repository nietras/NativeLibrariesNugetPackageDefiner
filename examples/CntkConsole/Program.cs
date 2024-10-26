using System;
using CNTK;
Console.WriteLine(Environment.CurrentDirectory);
var device = DeviceDescriptor.GPUDevice(0);
Console.WriteLine(device.Type);