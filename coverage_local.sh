#!/bin/bash

set -e

OPENCOVER=C:/build/OpenCover/OpenCover.Console.exe
REPORTGENERATOR=C:/build/ReportGenerator/ReportGenerator.exe

CONFIG=Release
# Arguments to use for the build
DOTNET_BUILD_ARGS="-c $CONFIG"
# Arguments to use for the test
DOTNET_TEST_ARGS="$DOTNET_BUILD_ARGS"

echo CLI args: $DOTNET_BUILD_ARGS

echo Restoring
#dotnet restore -v Warning

echo Building
#dotnet build $DOTNET_BUILD_ARGS **/project.json

echo Testing

COVERAGE_DIR=./coverage/report
COVERAGE_HISTORY_DIR=./coverage/history
rm -rf $COVERAGE_DIR
mkdir $COVERAGE_DIR

echo "Calculating coverage with OpenCover"

PROJECTS=(\
"ServiceBase.IdentityServer.EntityFramework.IntegrationTests" \
"ServiceBase.IdentityServer.EntityFramework.UnitTests" \
"ServiceBase.IdentityServer.Public.IntegrationTests" \
"ServiceBase.IdentityServer.Public.UnitTests" \
"ServiceBase.IdentityServer.UnitTests" \
)

for PROJECT in "${PROJECTS[@]}"
do
   :
$OPENCOVER \
  -target:"c:\Program Files\dotnet\dotnet.exe" \
  -targetargs:"test -f netcoreapp1.1 $DOTNET_TEST_ARGS test/$PROJECT" \
  -mergeoutput \
  -hideskipped:File \
  -output:$COVERAGE_DIR/coverage.xml \
  -oldStyle \
  -filter:"+[ServiceBase*]* -[ServiceBase.*Tests*]*" \
  -searchdirs:$testdir/bin/$CONFIG/netcoreapp1.1 \
  -register:user
done

echo "Generating HTML report"
$REPORTGENERATOR \
  -reports:$COVERAGE_DIR/coverage.xml \
  -targetdir:$COVERAGE_DIR \
  -historydir:$COVERAGE_HISTORY_DIR \
  -reporttypes:"Html" \
  -verbosity:Error
