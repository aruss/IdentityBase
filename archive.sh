#!/bin/bash

DIR="$( cd "$( dirname "$0" )" && pwd )"
RUNTIME="linux-x64"
VERSION="2.0.0"

tar -czvf "$DIR/build/identitybase-$VERSION.tgz" "$DIR/build/$RUNTIME/identitybase-$VERSION"