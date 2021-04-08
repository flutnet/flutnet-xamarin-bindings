#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/../tools
NUGET_EXE=$TOOLS_DIR/nuget/nuget.exe

# Clear the NuGet package cache, NuGet temp files and NuGet http cache
mono "$NUGET_EXE" locals all -clear