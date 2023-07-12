
// File versioning is a joke, so need some other way to version the files.
// Perhaps presence of `.version` file can be used and if none try to get from `FileVersionInfo`.
// get-childitem * -include *.dll,*.exe | foreach-object { "{0}`t{1}" -f $_.Name, [System.Diagnostics.FileVersionInfo]::GetVersionInfo($_).FileVersion }

using static NativeLibrariesNugetPackageDefiner.NugetPackageDefiner;

Run();

// Probably put in directory structure
//const string LicenseText = "This software contains source code provided by NVIDIA Corporation.\r\nThe full cuDNN license agreement can be found here.\r\nhttps://docs.nvidia.com/deeplearning/cudnn/sla/index.html";
