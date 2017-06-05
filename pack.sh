#!/bin/bash

nuget pack ./src/IdentityBase/IdentityBase.nuspec -OutputDirectory ./artifacts/packages -version $APPVEYOR_BUILD_VERSION
nuget pack ./src/IdentityBase.Public/IdentityBase.Public.nuspec -OutputDirectory ./artifacts/packages -version $APPVEYOR_BUILD_VERSION
nuget pack ./src/IdentityBase.Public.EntityFramework/IdentityBase.Public.EntityFramework.nuspec -OutputDirectory ./artifacts/packages -version $APPVEYOR_BUILD_VERSION