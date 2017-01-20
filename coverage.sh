#!/bin/bash

set -e

# Install OpenCover and ReportGenerator, and save the path to their executables.
nuget install -Verbosity quiet -OutputDirectory packages -Version 4.6.519 OpenCover
nuget install -Verbosity quiet -OutputDirectory packages -Version 2.4.5.0 ReportGenerator

OPENCOVER=$PWD/packages/OpenCover.4.6.519/tools/OpenCover.Console.exe
REPORTGENERATOR=$PWD/packages/ReportGenerator.2.4.5.0/tools/ReportGenerator.exe

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

COVERAGE_DIR=./coverage
rm -rf $COVERAGE_DIR
mkdir $COVERAGE_DIR

PROJECTS=(\
"ServiceBase.IdentityServer.EntityFramework.IntegrationTests" \
"ServiceBase.IdentityServer.EntityFramework.UnitTests" \
"ServiceBase.IdentityServer.Public.IntegrationTests" \
"ServiceBase.IdentityServer.Public.UnitTests" \
"ServiceBase.IdentityServer.UnitTests"\
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
  -filter:"+[ServiceBase.IdentityServer*]* -[ServiceBase.IdentityServer.*Tests*]*" \
  -searchdirs:$testdir/bin/$CONFIG/netcoreapp1.1 \
  -register:user
done