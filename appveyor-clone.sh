#!/bin/bash

git clone -q --branch=$APPVEYOR_REPO_BRANCH https://github.com/$APPVEYOR_REPO_NAME.git $APPVEYOR_BUILD_FOLDER
git checkout -qf $APPVEYOR_REPO_COMMIT
git submodule update --init