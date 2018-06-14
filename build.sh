#!/bin/bash

DIR="$( cd "$( dirname "$0" )" && pwd )"
RUNTIME="linux-x64"
VERSION="2.0.0"
SOURCEDIR=$DIR/src/IdentityBase.Web
BUILDDIR=$DIR/build/$RUNTIME/identitybase-$VERSION

echo "Cleanup old build files \"$BUILDDIR\""
rm -rf $BUILDDIR
mkdir -p $BUILDDIR

echo "Copy distribution files"
cp -r $DIR/distribution/$RUNTIME/. $BUILDDIR

echo "Cleanup, restore and compile host application"
rm -rf $SOURCEDIR/bin
rm -rf $SOURCEDIR/obj
dotnet publish $SOURCEDIR/IdentityBase.Web.csproj -c Release -r $RUNTIME -o $BUILDDIR/lib --force

# Get a list of all host application assemblies
HOSTASSEMBLIES=$( ls $BUILDDIR/lib/*.* )

# Loop throw all plugin source folders
for PATH1 in $SOURCEDIR/Plugins/*/ ; do

	PLUGIN=$(basename $PATH1)
	PLUGINSOURCEDIR=$SOURCEDIR/Plugins/$PLUGIN
    PLUGINBUILDDIR=$BUILDDIR/plugins/$PLUGIN

    echo $PLUGIN

	# Cleanup, restore and compile plugins
	rm -rf $PLUGINSOURCEDIR/bin
    rm -rf $PLUGINSOURCEDIR/obj
	dotnet publish $PLUGINSOURCEDIR/$PLUGIN.csproj -c Release -r $RUNTIME -o $PLUGINBUILDDIR --force

	# Remove assemblies from plugin directories that a present in host application
    for PATH2 in $HOSTASSEMBLIES ; do

        FILE=$(basename $PATH2)
        rm $PLUGINBUILDDIR/$FILE
    done

    rm -rf $PLUGINBUILDDIR/refs
    rm $PLUGINBUILDDIR/*.pdb
    rm $PLUGINBUILDDIR/apphost
done