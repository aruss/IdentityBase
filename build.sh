#!/bin/bash

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
BUILDDIR=$DIR/build/$RUNTIME/identitybase-$VERSION

echo "Source directory: \"$SOURCEDIR\""
echo "Buid directory: \"$BUILDDIR\""

echo "Cleanup old build files \"$BUILDDIR\""
rm -rf $BUILDDIR
mkdir -p $BUILDDIR

echo "Copy distribution files"
cp -r $DIR/distribution/$RUNTIME/. $BUILDDIR

echo "Cleanup, restore and compile host application"
rm -rf $SOURCEDIR/bin 2> /dev/null
rm -rf $SOURCEDIR/obj 2> /dev/null
dotnet publish $SOURCEDIR/IdentityBase.Web.csproj -c Release -r $RUNTIME -o $BUILDDIR/lib --force

# Get a list of all host application assemblies
HOSTASSEMBLIES=$( ls $BUILDDIR/lib/*.* )

echo "Loop throw all plugin source folders"
for PATH1 in $SOURCEDIR/Plugins/*/ ; do

	PLUGIN=$(basename $PATH1)
	PLUGINSOURCEDIR=$SOURCEDIR/Plugins/$PLUGIN
    PLUGINBUILDDIR=$BUILDDIR/plugins/$PLUGIN

	echo "Cleanup, restore and compile plugins"
	rm -rf $PLUGINSOURCEDIR/bin 2> /dev/null
    rm -rf $PLUGINSOURCEDIR/obj 2> /dev/null
	dotnet publish $PLUGINSOURCEDIR/$PLUGIN.csproj -c Release -r $RUNTIME -o $PLUGINBUILDDIR --force

	echo "Removing assemblies from plugin directories that a present in host application"
    for PATH2 in $HOSTASSEMBLIES ; do

        FILE=$(basename $PATH2)
        rm $PLUGINBUILDDIR/$FILE 2> /dev/null
    done

    rm -rf $PLUGINBUILDDIR/refs 2> /dev/null
    rm $PLUGINBUILDDIR/*.pdb 2> /dev/null
    rm $PLUGINBUILDDIR/apphost 2> /dev/null
    rm $PLUGINBUILDDIR/apphost.exe 2> /dev/null
done

echo "Building $RUNTIME v$VERSION successfully done"