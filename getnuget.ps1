$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
# We download into packages so it is not checked-in since this is ignored in git
$packagesPath = ".\pkgs\"
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