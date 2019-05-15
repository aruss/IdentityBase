docker run -it --rm ^
    -v %cd%:/IdentityBase ^
    -v %userprofile%/AppData/Local/NuGet/v3-cache:/root/.local/share/NuGet/v3-cache ^
    -v %userprofile%/.nuget/packages:/root/.nuget/packages ^
    mcr.microsoft.com/dotnet/core/sdk:2.2 ^
    sh -c "cd /IdentityBase && chmod +x build.sh && sh ./build.sh win7-x64 latest"