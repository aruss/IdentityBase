#!/bin/bash

nuget pack ./src/IdentityBase/IdentityBase.nuspec -OutputDirectory ./artifacts/packages -suffix $APPVEYOR_BUILD_NUMBER
nuget pack ./src/IdentityBase.Public/IdentityBase.Public.nuspec -OutputDirectory ./artifacts/packages -suffix $APPVEYOR_BUILD_NUMBER
nuget pack ./src/IdentityBase.Public.EntityFramework/IdentityBase.Public.EntityFramework.nuspec -OutputDirectory ./artifacts/packages -suffix $APPVEYOR_BUILD_NUMBER