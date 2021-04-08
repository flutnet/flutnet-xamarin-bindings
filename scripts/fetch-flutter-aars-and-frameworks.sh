#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/../tools
ASSETS_DIR=$SCRIPT_DIR/../assets

"$TOOLS_DIR/FlutterSync/FlutterSync" --TargetDirectory "$ASSETS_DIR/xamarin-native-references"
