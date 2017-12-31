#!/bin/bash

set -e

nuget install -Verbosity quiet -OutputDirectory packages -Version 4.6.589 OpenCover -Source $PWD/tools
nuget install -Verbosity quiet -OutputDirectory packages -Version 2.5.8.0 ReportGenerator

OPENCOVER=$PWD/packages/OpenCover.4.6.589/tools/OpenCover.Console.exe
REPORTGENERATOR=$PWD/packages/ReportGenerator.2.5.8/tools/ReportGenerator.exe
COVERAGE_DIR=./coverage/report
COVERAGE_HISTORY_DIR=./coverage/history

rm -rf $COVERAGE_DIR
mkdir -p $COVERAGE_DIR

PROJECTS=(\
#"IdentityBase.EntityFramework.IntegrationTests\IdentityBase.EntityFramework.IntegrationTests.csproj" \
#"IdentityBase.EntityFramework.UnitTests\IdentityBase.EntityFramework.UnitTests.csproj" \
"IdentityBase.IntegrationTests\IdentityBase.IntegrationTests.csproj" \
#"IdentityBase.UnitTests\IdentityBase.UnitTests.csproj"
)

for PROJECT in "${PROJECTS[@]}"
do
   :
$OPENCOVER \
  -target:"c:\Program Files\dotnet\dotnet.exe" \
  -targetargs:"test -f netcoreapp2.0 -c Release ./test/$PROJECT" \
  -mergeoutput \
  -hideskipped:File \
  -output:$COVERAGE_DIR/coverage.xml \
  -oldStyle \
  -filter:"+[IdentityBase*]* -[IdentityBase.*Tests*]*" \
  -searchdirs:./test/$PROJECT/bin/Release/netcoreapp2.0 \
  -register:user
done

echo "Generating HTML report"
$REPORTGENERATOR \
  -reports:$COVERAGE_DIR/coverage.xml \
  -targetdir:$COVERAGE_DIR \
  -historydir:$COVERAGE_HISTORY_DIR \
  -reporttypes:"Html" \
  -verbosity:Error