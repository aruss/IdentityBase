#!/bin/bash

nuget pack ./src/IdentityBase.Public/IdentityBase.Public.nuspec -OutputDirectory ./artifacts/packages -version $APPVEYOR_BUILD_VERSION
