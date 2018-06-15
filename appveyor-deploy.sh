#!/bin/bash

DIR="$( cd "$( dirname "$0" )" && pwd )"

echo "publish docker image"
docker login -u="$DOCKER_USER" -p="$eDOCKER_PASS"
docker push identitybasenet/identitybase:$APPVEYOR_BUILD_VERSION

echo "create GitHub release"
sudo $DIR/tools/ghr -t "$GITHUB_TOKEN" -u "IdentityBaseNet" -r "IdentityBase" -c "$APPVEYOR_REPO_COMMIT" -delete -draft v$APPVEYOR_BUILD_VERSION $DIR/build