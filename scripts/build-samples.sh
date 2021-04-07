#!/usr/bin/env bash

# Define directories
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
SAMPLES_DIR=$SCRIPT_DIR/../samples
VSTOOL_PATH="/Applications/Visual Studio.app/Contents/MacOS/vstool"

# Build Flutter module
cd "$SAMPLES_DIR/flutter_module"
flutter pub get
flutter build aar --no-profile
flutter build ios-framework --no-profile

# Build sample projects
# cd "$SAMPLES_DIR"
"$VSTOOL_PATH" build --configuration:Debug "$SAMPLES_DIR/Flutnet.Interop.Samples.sln"
"$VSTOOL_PATH" build --configuration:Release "$SAMPLES_DIR/Flutnet.Interop.Samples.sln"