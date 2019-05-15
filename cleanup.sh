#!/bin/bash

# this script removes all the build artificats
# from the main app and all the plugins.

DIR="$( cd "$( dirname "$0" )" && pwd )"
SOURCEDIR=$DIR/src/IdentityBase.Web

echo "Removing $SOURCEDIR/bin"
rm -rf $SOURCEDIR/bin

echo "Removing $SOURCEDIR/obj"
rm -rf $SOURCEDIR/obj

# Loop throw all plugin source folders
for PATH1 in $SOURCEDIR/Plugins/*/ ; do

	PLUGIN=$(basename $PATH1)
	PLUGINSOURCEDIR=$SOURCEDIR/Plugins/$PLUGIN

	echo "Removing $PLUGINSOURCEDIR/bin"
	rm -rf $PLUGINSOURCEDIR/bin

	echo "Removing $PLUGINSOURCEDIR/obj"
    rm -rf $PLUGINSOURCEDIR/obj

done