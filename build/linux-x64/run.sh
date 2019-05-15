#!/bin/sh

DIR="$( cd "$( dirname "$0" )" && pwd )"

if [ -z ${ASPNETCORE_CONTENTROOT+x} ]; then
  echo "setting up contentroot"
  export ASPNETCORE_CONTENTROOT=$DIR
fi

if [ -z ${ASPNETCORE_CONFIGROOT+x} ]; then
  export ASPNETCORE_CONFIGROOT=$DIR/config
fi

if [ -z ${ASPNETCORE_ENVIRONMENT+x} ]; then
  export ASPNETCORE_ENVIRONMENT=Production
fi

echo "Starting IdentityBase"
echo "   DIR: " $DIR
echo "   Content root:" $ASPNETCORE_CONTENTROOT
echo "   Config root:" $ASPNETCORE_CONFIGROOT
echo "   Environment:" $ASPNETCORE_ENVIRONMENT

dotnet $DIR/lib/IdentityBase.Web.dll
