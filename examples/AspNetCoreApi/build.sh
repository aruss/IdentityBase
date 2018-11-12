dotnet publish AspNetCoreApi.csproj -c Release -r linux-x64
docker build . -t identitybasenet/aspnetcoreapi:latest