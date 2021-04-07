#!/bin/bash

# Define directories
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
ARTIFACTS_DIR=$SCRIPT_DIR/../artifacts
SRC_DIR=$SCRIPT_DIR/../src
TOOLS_DIR=$SCRIPT_DIR/../tools
NUGET_EXE=$TOOLS_DIR/nuget/nuget.exe

# Create NuGet packages for binding libraries
mono "$NUGET_EXE" pack "$SRC_DIR/Flutnet.Interop.Android/Flutnet.Interop.Android.nuspec" -OutputDirectory "$ARTIFACTS_DIR/nuget-packages"
mono "$NUGET_EXE" pack "$SRC_DIR/Flutnet.Interop.iOS/Flutnet.Interop.iOS.nuspec" -OutputDirectory "$ARTIFACTS_DIR/nuget-packages"
mono "$NUGET_EXE" pack "$SRC_DIR/Flutnet.Interop.Java8/Flutnet.Interop.Java8.nuspec" -OutputDirectory "$ARTIFACTS_DIR/nuget-packages"
