#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
SRC_DIR=$SCRIPT_DIR/../src
VSTOOL_PATH="/Applications/Visual Studio.app/Contents/MacOS/vstool"
SOLUTION_PATH=$SCRIPT_DIR/../Flutnet.Interop.sln

# Clean and build binding libraries for all the available configurations
"$VSTOOL_PATH" build --configuration:Debug --target:Clean "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:ReleaseWithDebugNativeRef --target:Clean "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:Release --target:Clean "$SOLUTION_PATH"

"$VSTOOL_PATH" build --configuration:Debug "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:ReleaseWithDebugNativeRef "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:Release "$SOLUTION_PATH"