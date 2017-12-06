#!/bin/bash

DIR="$( cd "$( dirname "$0" )" && pwd )"

# prepare docker context
dotnet publish $DIR/../src/IdentityBase/IdentityBase.csproj -c Release -o $DIR/app

# build docker container
docker build . -t identitybasenet/identitybase

# cleanup docker context
rm -rf $DIR/app
