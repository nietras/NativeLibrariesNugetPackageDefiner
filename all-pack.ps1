./getnuget.ps1
$nuspecs = gci -Recurse ./defs/*.nuspec
ForEach ($nuspec in $nuspecs)
{
   $command = "./pkgs/nuget.exe pack $nuspec -OutputDirectory ./pkgs/"
   write-host $command
   iex $command
}