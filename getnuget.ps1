#!/usr/bin/env pwsh
# win-x86 only for now (download manually to directory and this will be skipped)
$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
# Download into specific directory so it is not checked-in since this is ignored in git
$packagesPath = "./pkgs/"
$targetNugetExe = $packagesPath + "nuget.exe"
# Download if it does not exist
If (!(Test-Path $targetNugetExe))
{
   If (!(Test-Path $packagesPath))
   {
     mkdir $packagesPath
   }
   "Downloading nuget to: " + $targetNugetExe
   Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
}