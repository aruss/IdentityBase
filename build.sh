#!/bin/bash

git submodule update --init --recursive
dotnet --info
dotnet restore --no-cache
dotnet build ./IdentityBase.sln --configuration Release