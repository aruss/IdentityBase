#!/bin/bash

#git submodule update --init --recursive
dotnet --info
dotnet restore --no-cache
dotnet build ./IdentityBase.sln --configuration Release

rm -rf ./identitybase-public-ctx && \
	cd ../src/IdentityBase.Public && \
	dotnet publish -c Release -o ../../docker/identitybase-public-ctx/app && \
	cd ../../docker && \
	cp ./identitybase-public.dockerfile ./identitybase-public-ctx/ && \
	cd ./identitybase-public-ctx && \
	docker build -f ./identitybase-public.dockerfile -t docker.econduct.de/identitybase/identitybase . && \
	docker push docker.econduct.de/identitybase/identitybase