# Getting Started - Development

IdentityBase is built against [ASP.NET Core 2.2](https://dotnet.microsoft.com/download) using the tooling that ships with Visual Studio 2017 on Windows 10.

### Requirements

* [Visual Studio 2017](https://www.visualstudio.com/de/vs/community)
* [.NET Core 2.2](https://www.microsoft.com/net/download/core#/current)
* [Docker](https://www.docker.com/docker-windows)

### How to build

run `build-docker.bat` or `build-win7x64.bat` to build IdentityBase
or use Visual Studio.

If you on linux just run following from the repository main directory

    docker build . -t identitybasenet/identitybase:latest