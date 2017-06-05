#!/bin/bash

dotnet --info
dotnet restore --no-cache
dotnet build ./IdentityBase.sln --configuration Release