#!/bin/bash

echo "build windows version"
sh build.sh win7-x64 $APPVEYOR_BUILD_VERSION

echo "pack windows release files: $APPVEYOR_BUILD_FOLDER/build/win7-x64/identitybase-$APPVEYOR_BUILD_VERSION -> $APPVEYOR_BUILD_FOLDER/build/identitybase-$APPVEYOR_BUILD_VERSION.zip"
cd ./build/win7-x64/ && zip -rq ../identitybase-$APPVEYOR_BUILD_VERSION.zip ./identitybase-$APPVEYOR_BUILD_VERSION && cd -

echo "build linux version"
sh build.sh linux-x64 $APPVEYOR_BUILD_VERSION

echo "pack linux release files: $APPVEYOR_BUILD_FOLDER/build/linux-x64/identitybase-$APPVEYOR_BUILD_VERSION -> $APPVEYOR_BUILD_FOLDER/build/identitybase-$APPVEYOR_BUILD_VERSION.tgz"
tar -czf $APPVEYOR_BUILD_FOLDER/build/identitybase-$APPVEYOR_BUILD_VERSION.tgz $APPVEYOR_BUILD_FOLDER/build/linux-x64/identitybase-$APPVEYOR_BUILD_VERSION

echo "build docker image"
cp $APPVEYOR_BUILD_FOLDER/distribution/Dockerfile $APPVEYOR_BUILD_FOLDER/build/linux-x64
docker build $APPVEYOR_BUILD_FOLDER/build/linux-x64 --build-arg VERSION=$APPVEYOR_BUILD_VERSION -t identitybasenet/identitybase:$APPVEYOR_BUILD_VERSION