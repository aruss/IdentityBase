#!/bin/bash

#DIR="$( cd "$( dirname "$0" )" && pwd )"

# prepare docker context
cp ./distribution/Dockerfile ./build/linux-x64

# build docker container
docker build ./build/linux-x64 -t identitybasenet/identitybase