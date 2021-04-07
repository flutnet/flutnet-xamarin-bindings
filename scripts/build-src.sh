#!/usr/bin/env bash

# Define directories
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
VSTOOL_PATH="/Applications/Visual Studio.app/Contents/MacOS/vstool"

# Build binding libraries for all the available configurations
"$VSTOOL_PATH" build --configuration:Debug "$SCRIPT_DIR/../Flutnet.Interop.sln"
"$VSTOOL_PATH" build --configuration:ReleaseWithDebugNativeRef "$SCRIPT_DIR/../Flutnet.Interop.sln"
"$VSTOOL_PATH" build --configuration:Release "$SCRIPT_DIR/../Flutnet.Interop.sln"