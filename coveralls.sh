#!/bin/bash

set -e

nuget install -Verbosity quiet -OutputDirectory packages -Version 4.6.589 OpenCover -Source $PWD/artifacts

OPENCOVER=$PWD/packages/OpenCover.4.6.589/tools/OpenCover.Console.exe
COVERAGE_DIR=./coverage

rm -rf $COVERAGE_DIR
mkdir $COVERAGE_DIR

PROJECTS=(\
"ServiceBase.IdentityServer.Public.EntityFramework.IntegrationTests\ServiceBase.IdentityServer.Public.EntityFramework.IntegrationTests.csproj" \
"ServiceBase.IdentityServer.Public.EntityFramework.UnitTests\ServiceBase.IdentityServer.Public.EntityFramework.UnitTests.csproj" \
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

if [ -n "$COVERALLS_REPO_TOKEN" ]
then
  nuget install -OutputDirectory packages -Version 0.7.0 coveralls.net
  packages/coveralls.net.0.7.0/tools/csmacnz.Coveralls.exe --opencover -i coverage/coverage.xml --useRelativePaths
fi