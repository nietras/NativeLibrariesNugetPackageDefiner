#!/usr/bin/env pwsh
./getnuget.ps1

$nuspecs = Get-ChildItem -Recurse ./defs/*.nuspec
$nugetExePath = Join-Path $PSScriptRoot "pkgs/nuget.exe"
$outputDirectory = Join-Path $PSScriptRoot "pkgs/"

# Use workflow to try to parallize nuget packs
workflow PackNuspecs {
    param($nugetExePath, $nuspecs, $outputDirectory)
    
    foreach -parallel ($nuspec in $nuspecs) {
        InlineScript {
            $nuspecPath = $using:nuspec.FullName
            $outputDir = $using:outputDirectory
            $command = "$using:nugetExePath pack $nuspecPath -OutputDirectory $outputDir"
            Write-Output "Executing command: $command"
            & $using:nugetExePath pack $nuspecPath -OutputDirectory $outputDir
        }
    }
}

PackNuspecs -nugetExePath $nugetExePath -nuspecs $nuspecs -outputDirectory $outputDirectory