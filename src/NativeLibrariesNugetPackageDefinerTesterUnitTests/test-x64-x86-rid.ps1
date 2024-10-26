#!/usr/bin/env pwsh
Write-Host "Testing Debug win-x86"
dotnet test --nologo -c Debug --runtime win-x86
Write-Host "Testing Release win-x86"
dotnet test --nologo -c Release --runtime win-x86
Write-Host "Testing Debug win-x64" 
dotnet test --nologo -c Debug --runtime win-x64
Write-Host "Testing Release win-x64"
dotnet test --nologo -c Release --runtime win-x64
