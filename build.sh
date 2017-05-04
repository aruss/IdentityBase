#!/bin/bash

git submodule update --init --recursive
dotnet --info
dotnet restore --no-cache
dotnet build .\ServiceBase.IdentityServer.sln --configuration Release