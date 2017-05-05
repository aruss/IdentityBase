#!/bin/bash

#git submodule update --init --recursive
#dotnet --info
#dotnet restore --no-cache
#dotnet build ./ServiceBase.IdentityServer.sln --configuration Release

rm -rf ./identitybase-public-ctx && \
cd ../src/ServiceBase.IdentityServer.Public && \
dotnet publish -c Release -o ../../docker/identitybase-public-ctx/app && \
cd ../../docker && \
cp ./identitybase-public.dockerfile ./identitybase-public-ctx/ && \
cd ./identitybase-public-ctx && \
docker build -f ./identitybase-public.dockerfile -t servicebase/identitybase .

#docker run -it --rm -p 5000:5000 servicebase/identitybase