REM docker run -it --rm -v %cd%:/idbase microsoft/dotnet:2.1-sdk sh -c "cd /idbase && chmod +x build.sh && sh ./build.sh linux-x64 2.0.0"

docker run -it --rm -v %cd%:/idbase microsoft/dotnet:2.1-sdk 