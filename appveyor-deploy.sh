#!/bin/bash

echo "publish docker image"
docker login --username=$DOCKER_USER --password=$DOCKER_PASS
docker push identitybasenet/identitybase:$APPVEYOR_BUILD_VERSION

echo "create GitHub release"
cd $APPVEYOR_BUILD_FOLDER/tools && \
  chmod +x ghr && \
  ./ghr -t "$GITHUB_TOKEN" -u "IdentityBaseNet" -r "IdentityBase" -c "$APPVEYOR_REPO_COMMIT" -delete v$APPVEYOR_BUILD_VERSION $APPVEYOR_BUILD_FOLDER/build && \
  cd -