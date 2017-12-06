#!/bin/bash

nuget pack ./src/IdentityBase/IdentityBase.nuspec -OutputDirectory ./artifacts/packages -version $APPVEYOR_BUILD_VERSION
