#!/bin/bash

DIR="$( cd "$( dirname "$0" )" && pwd )"

echo "publish docker image"
docker login --username=$DOCKER_USER --password=$DOCKER_PASS
docker push identitybasenet/identitybase:$APPVEYOR_BUILD_VERSION

echo "create GitHub release"

echo "debug -------------------------------------------"
echo "DIR = $DIR"
cd ls $DIR/tools -l
cd ls $DIR/build -l
ehco "debug -------------------------------------------"

sudo $DIR/tools/ghr -t "$GITHUB_TOKEN" -u "IdentityBaseNet" -r "IdentityBase" -c "$APPVEYOR_REPO_COMMIT" -delete -draft v$APPVEYOR_BUILD_VERSION $DIR/build