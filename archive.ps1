#!/usr/bin/env powershell
#requires -version 4

$Runtime = "win7-x64"
$Version = "2.0.0"
$BuildDir =  "$PSScriptRoot\build\identitybase-$Version.zip"

Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$BuildDir"
Compress-Archive -Path "$PSScriptRoot\build\$Runtime\identitybase-$Version" -Update -DestinationPath "$BuildDir"