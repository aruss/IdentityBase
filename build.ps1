#!/usr/bin/env powershell
#requires -version 4

$Runtime = "win7-x64"
$Version = "2.0.0"
$SourceDir = "$PSScriptRoot\src\IdentityBase.Web"
$BuildDir = "$PSScriptRoot\build\$Runtime\identitybase-$Version"

# Cleanup old build
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$BuildDir"

# Copy distribution files
Copy-Item -Path "$PSScriptRoot\distribution\$Runtime" -Recurse -Destination "$BuildDir" -Container

# Cleanup, restore and compile host application
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$SourceDir\bin"
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$SourceDir\obj"
dotnet publish "$SourceDir\IdentityBase.Web.csproj" -c Release -r $Runtime -o "$BuildDir\lib" --force

# Get a list of all host application assemblies
$HostAssemblies = Get-ChildItem "$BuildDir\lib" -Filter *.dll

# Loop throw all plugin source folders
$Plugins = Get-ChildItem -Path "$SourceDir\Plugins" -Directory -Force -ErrorAction SilentlyContinue
foreach ($Plugin in $Plugins)
{
    $PluginSourceDir = "$SourceDir\Plugins\$Plugin";
    $PluginBuildDir = "$BuildDir\plugins\$Plugin";

    # Cleanup, restore and compile all plugins
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$PluginSourceDir\bin"
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$PluginSourceDir\obj"
    dotnet publish "$PluginSourceDir\$Plugin.csproj" -c Release -r $Runtime -o "$PluginBuildDir" --force

    # Remove assemblies from plugin directories that a present in host application
    foreach ($HostAssembly in $HostAssemblies)
    {
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$PluginBuildDir\$HostAssembly"
    }

    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$PluginBuildDir\refs"
    Remove-Item -Force -ErrorAction SilentlyContinue "$PluginBuildDir\*.pdb"
    Remove-Item -Force -ErrorAction SilentlyContinue "$PluginBuildDir\apphost.exe"
}