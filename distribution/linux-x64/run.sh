#!/bin/bash

DIR="$( cd "$( dirname "$0" )" && pwd )"

if ! [[ -v ASPNETCORE_CONTENTROOT ]]; then
  export ASPNETCORE_CONTENTROOT=$DIR
fi

if ! [[ -v ASPNETCORE_CONFIGROOT ]]; then
  export ASPNETCORE_CONFIGROOT=$DIR/config
fi

if ! [[ -v ASPNETCORE_ENVIRONMENT ]]; then
  export ASPNETCORE_ENVIRONMENT=Production
fi

dotnet $DIR/lib/IdentityBase.Web.dll