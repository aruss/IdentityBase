#!/bin/bash

set -e

nuget install -Verbosity quiet -OutputDirectory packages -Version 4.6.589 OpenCover -Source $PWD/tools

OPENCOVER=$PWD/packages/OpenCover.4.6.589/tools/OpenCover.Console.exe
COVERAGE_DIR=./coverage

rm -rf $COVERAGE_DIR
mkdir $COVERAGE_DIR

PROJECTS=(\
"IdentityBase.EntityFramework.IntegrationTests\IdentityBase.EntityFramework.IntegrationTests.csproj" \
"IdentityBase.EntityFramework.UnitTests\IdentityBase.EntityFramework.UnitTests.csproj" \
"IdentityBase.IntegrationTests\IdentityBase.IntegrationTests.csproj" \
"IdentityBase.UnitTests\IdentityBase.UnitTests.csproj")

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

if [ -n "$COVERALLS_REPO_TOKEN" ]
then
  nuget install -OutputDirectory packages -Version 0.7.0 coveralls.net
  packages/coveralls.net.0.7.0/tools/csmacnz.Coveralls.exe --opencover -i coverage/coverage.xml --useRelativePaths
fi