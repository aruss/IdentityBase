#!/bin/bash

# move files to git releases
# publish docker image

#docker login -u="$DOCKER_USER" -p="$eDOCKER_PASS"
#docker push identitybasenet/identitybase:$APPVEYOR_BUILD_VERSION

echo "create GitHub release"
sudo ./tools/ghr -t "$GITHUB_TOKEN" -u "IdentityBaseNet" -r "IdentityBase" -c "$APPVEYOR_REPO_COMMIT" -delete -draft v$APPVEYOR_BUILD_VERSION ./build