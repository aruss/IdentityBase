docker run -it --rm ^
    -v %cd%:/idbase ^
    -v %userprofile%/AppData/Local/NuGet/v3-cache:/root/.local/share/NuGet/v3-cache ^
    -v %userprofile%/.nuget/packages:/root/.nuget/packages ^
    -v %userprofile%/AppData/Local/Temp/NuGetScratch:/tmp/NuGetScratch ^
    microsoft/dotnet:2.1-sdk ^
    sh -c "cd /idbase && chmod +x build.sh && sh ./build.sh linux-x64 latest"

xcopy %cd%\distribution\Dockerfile %cd%\build\linux-x64

docker build %cd%\build\linux-x64 --build-arg VERSION=latest -t identitybasenet/identitybase:latest