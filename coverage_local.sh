#!/bin/bash

set -e

nuget install -Verbosity quiet -OutputDirectory packages -Version 4.6.589 OpenCover -Source $PWD/artifacts
nuget install -Verbosity quiet -OutputDirectory packages -Version 2.4.5.0 ReportGenerator

OPENCOVER=$PWD/packages/OpenCover.4.6.589/tools/OpenCover.Console.exe
REPORTGENERATOR=$PWD/packages/ReportGenerator.2.4.5.0/tools/ReportGenerator.exe
COVERAGE_DIR=./coverage/report
COVERAGE_HISTORY_DIR=./coverage/history

rm -rf $COVERAGE_DIR
mkdir -p $COVERAGE_DIR

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

echo "Generating HTML report"
$REPORTGENERATOR \
  -reports:$COVERAGE_DIR/coverage.xml \
  -targetdir:$COVERAGE_DIR \
  -historydir:$COVERAGE_HISTORY_DIR \
  -reporttypes:"Html" \
  -verbosity:Error

  
