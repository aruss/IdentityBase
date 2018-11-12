dotnet publish AspNetCoreWeb.csproj -c Release -r linux-x64
docker build . -t identitybasenet/aspnetcoreweb:latest