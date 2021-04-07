#!/usr/bin/env bash

# Define directories
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/../tools
NUGET_EXE=$TOOLS_DIR/nuget/nuget.exe

# Clear the NuGet package cache, NuGet temp files and its http cache
mono "$NUGET_EXE" locals all -clear