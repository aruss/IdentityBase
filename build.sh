#!/bin/bash

# this script builds the .net application with specific runtime and version

DIR="$( cd "$( dirname "$0" )" && pwd )"

echo "Working directory: \"$DIR\""

# checking for runtime ./build.sh linux-x64
if [ "$1" != "" ]; then
    RUNTIME=$1
else
    echo "Runtime is not specified"
    exit 1
fi

echo "Runtime: \"$RUNTIME\""

# checking for version ./build.sh linux-x64 2.0.0
if [ "$2" != "" ]; then
    VERSION=$2
else
    echo "Version is not specified"
    exit 1
fi

echo "Version: \"$VERSION\""

SOURCEDIR=$DIR/src/IdentityBase.Web
OUTPUTDIR=$DIR/artifacts/$RUNTIME/identitybase-$VERSION

echo "Source directory: \"$SOURCEDIR\""
echo "Output directory: \"$OUTPUTDIR\""

echo "Cleanup old artifacts \"$OUTPUTDIR\""
rm -rf $OUTPUTDIR
mkdir -p $OUTPUTDIR

echo "Copy distribution files"
cp -r $DIR/build/$RUNTIME/. $OUTPUTDIR

echo "Cleanup, restore and compile host application"
rm -rf $SOURCEDIR/bin 2> /dev/null
rm -rf $SOURCEDIR/obj 2> /dev/null
dotnet publish $SOURCEDIR/IdentityBase.Web.csproj -c Release -r $RUNTIME -o $OUTPUTDIR/lib

# Get a list of all host application assemblies
HOSTASSEMBLIES=$( ls $OUTPUTDIR/lib/*.* )

echo "Loop throw all plugin source folders"
for PATH1 in $SOURCEDIR/Plugins/*/; do

	PLUGIN=$(basename $PATH1)
	PLUGINSOURCEDIR=$SOURCEDIR/Plugins/$PLUGIN
    PLUGINOUTPUTDIR=$OUTPUTDIR/plugins/$PLUGIN

	echo "Cleanup, restore and compile plugins"
	rm -rf $PLUGINSOURCEDIR/bin 2> /dev/null
    rm -rf $PLUGINSOURCEDIR/obj 2> /dev/null
	dotnet publish $PLUGINSOURCEDIR/$PLUGIN.csproj -c Release -r $RUNTIME -o $PLUGINOUTPUTDIR

	echo "Removing assemblies from plugin directories that are present in host application"
    for PATH2 in $HOSTASSEMBLIES; do

        FILE=$(basename $PATH2)
        rm $PLUGINOUTPUTDIR/$FILE 2> /dev/null
    done

    rm -rf $PLUGINOUTPUTDIR/refs 2> /dev/null
    rm $PLUGINOUTPUTDIR/*.pdb 2> /dev/null
    rm $PLUGINOUTPUTDIR/apphost 2> /dev/null
    rm $PLUGINOUTPUTDIR/apphost.exe 2> /dev/null
done

echo "Building $RUNTIME v$VERSION successfully done"
