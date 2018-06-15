#!/bin/bash

DIR="$( cd "$( dirname "$0" )" && pwd )"

echo "install required tools"
apt-get update
apt-get --assume-yes install zip

echo "build windows version"
sh build.sh win7-x64 $APPVEYOR_BUILD_VERSION

echo "pack windows release files: $DIR/build/win7-x64/identitybase-$APPVEYOR_BUILD_VERSION -> $DIR/build/identitybase-$APPVEYOR_BUILD_VERSION.zip"
cd ./build/win7-x64/ && zip -rq ../identitybase-$APPVEYOR_BUILD_VERSION.zip ./identitybase-$APPVEYOR_BUILD_VERSION && cd -
#appveyor PushArtifact $DIR/build/identitybase-$APPVEYOR_BUILD_VERSION.zip

echo "build linux version"
sh build.sh linux-x64 $APPVEYOR_BUILD_VERSION

echo "pack linux release files: $DIR/build/linux-x64/identitybase-$APPVEYOR_BUILD_VERSION -> $DIR/build/identitybase-$APPVEYOR_BUILD_VERSION.tgz"
tar -czf $DIR/build/identitybase-$APPVEYOR_BUILD_VERSION.tgz $DIR/build/linux-x64/identitybase-$APPVEYOR_BUILD_VERSION
#appveyor PushArtifact $DIR/build/identitybase-$APPVEYOR_BUILD_VERSION.tgz

echo "build docker image"
cp $DIR/distribution/Dockerfile $DIR/build/linux-x64
docker build $DIR/build/linux-x64 --build-arg VERSION=$APPVEYOR_BUILD_VERSION -t identitybasenet/identitybase:$APPVEYOR_BUILD_VERSION