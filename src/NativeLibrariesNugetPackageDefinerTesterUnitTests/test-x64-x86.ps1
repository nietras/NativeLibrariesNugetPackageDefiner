#!/usr/bin/env pwsh
Write-Host "Testing Debug X86"
dotnet test --nologo -c Debug -- RunConfiguration.TargetPlatform=x86
Write-Host "Testing Release X86"
dotnet test --nologo -c Release -- RunConfiguration.TargetPlatform=x86
Write-Host "Testing Debug X64"
dotnet test --nologo -c Debug -- RunConfiguration.TargetPlatform=x64
Write-Host "Testing Release X64"
dotnet test --nologo -c Release -- RunConfiguration.TargetPlatform=x64
