#!/bin/bash

set -e

nuget install -Verbosity quiet -OutputDirectory packages -Version 4.6.589 OpenCover -Source $PWD/artifacts

OPENCOVER=$PWD/packages/OpenCover.4.6.589/tools/OpenCover.Console.exe
COVERAGE_DIR=./coverage

rm -rf $COVERAGE_DIR
mkdir $COVERAGE_DIR

PROJECTS=(\
"ServiceBase.IdentityServer.Public.EF.IntegrationTests\ServiceBase.IdentityServer.Public.EF.IntegrationTests.csproj" \
"ServiceBase.IdentityServer.Public.EF.UnitTests\ServiceBase.IdentityServer.Public.EF.UnitTests.csproj" \
"ServiceBase.IdentityServer.Public.IntegrationTests\ServiceBase.IdentityServer.Public.IntegrationTests.csproj" \
"ServiceBase.IdentityServer.Public.UnitTests\ServiceBase.IdentityServer.Public.UnitTests.csproj" \
"ServiceBase.IdentityServer.UnitTests\ServiceBase.IdentityServer.UnitTests.csproj")

for PROJECT in "${PROJECTS[@]}"
do
   :
$OPENCOVER \
  -target:"c:\Program Files\dotnet\dotnet.exe" \
  -targetargs:"test -f netcoreapp1.1 -c Release ./test/$PROJECT" \
  -mergeoutput \
  -hideskipped:File \
  -output:$COVERAGE_DIR/coverage.xml \
  -oldStyle \
  -filter:"+[ServiceBase.IdentityServer*]* -[ServiceBase.IdentityServer.*Tests*]*" \
  -searchdirs:./test/$PROJECT/bin/Release/netcoreapp1.1 \
  -register:user
done