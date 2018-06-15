#!/bin/bash

DIR="$( cd "$( dirname "$0" )" && pwd )"

# build windows version
#sh build.sh win7-x64 $APPVEYOR_BUILD_VERSION

# pack windows release
#sudo apt-get install zip

# build linux version
sh build.sh linux-x64 $APPVEYOR_BUILD_VERSION

# pack linux release files
tar -czvf $DIR/build/identitybase-$APPVEYOR_BUILD_VERSION.tgz $DIR/build/linux-x64/identitybase-$APPVEYOR_BUILD_VERSION

# build docker image
cp $DIR/distribution/Dockerfile $DIR/build/linux-x64
docker build $DIR/build/linux-x64 --build-arg VERSION=$APPVEYOR_BUILD_VERSION -t identitybasenet/identitybase:$APPVEYOR_BUILD_VERSION