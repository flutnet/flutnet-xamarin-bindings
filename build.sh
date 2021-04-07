#!/usr/bin/env bash

##########################################################################
# This is the bootstrapper for Cake runner for .NET Framework
# https://cakebuild.net/docs/running-builds/runners/cake-runner-for-dotnet-framework
##########################################################################

# Define directories
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
CAKE_ADDINS_DIR=$TOOLS_DIR/CakeAddins
CAKE_TOOLS_DIR=$TOOLS_DIR/CakeTools
CAKE_VERSION=0.38.5
CAKE_EXE=$TOOLS_DIR/Cake.$CAKE_VERSION/Cake.exe
NUGET_EXE=$TOOLS_DIR/nuget/nuget.exe

export CAKE_PATHS_ADDINS=$CAKE_ADDINS_DIR
export CAKE_PATHS_TOOLS=$CAKE_TOOLS_DIR

# Define default arguments
CAKE_SCRIPT=$SCRIPT_DIR/build.cake
CAKE_ARGUMENTS=()

# Download NuGet if it does not exist
if [ ! -f "$NUGET_EXE" ]; then
    echo "Downloading NuGet..."
    curl --create-dirs -Lsfo "$NUGET_EXE" https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
    if [ $? -ne 0 ]; then
        echo "An error occurred while downloading nuget.exe."
        exit 1
    fi
fi

# Download and install Cake if it does not exists
if [ ! -f "$CAKE_EXE" ]; then
    echo "Installing Cake..."
    curl -Lsfo Cake.zip "https://www.nuget.org/api/v2/package/Cake/$CAKE_VERSION" && unzip -q Cake.zip -d "$TOOLS_DIR/Cake.$CAKE_VERSION" && rm -f Cake.zip
    if [ $? -ne 0 ]; then
        echo "An error occured while installing Cake."
        exit 1
    fi
fi

# Make sure that Cake has been installed
if [ ! -f "$CAKE_EXE" ]; then
    echo "Could not find Cake.exe at '$CAKE_EXE'."
    exit 1
fi

# Start Cake
exec mono "$CAKE_EXE" $CAKE_SCRIPT "${CAKE_ARGUMENTS[@]}"

# Clean up environment variables that were created earlier in this bootstrapper
unset CAKE_ADDINS_DIR
unset CAKE_TOOLS_DIR