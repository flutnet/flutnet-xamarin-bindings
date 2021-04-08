#!/usr/bin/env bash

# Define paths
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
SAMPLES_DIR=$SCRIPT_DIR/../samples
VSTOOL_PATH="/Applications/Visual Studio.app/Contents/MacOS/vstool"
SOLUTION_PATH=$SAMPLES_DIR/Flutnet.Interop.Samples.sln

# Build Flutter module
cd "$SAMPLES_DIR/flutter_module"
flutter pub get
flutter build aar --no-profile
flutter build ios-framework --no-profile

# Clean and build sample projects
"$VSTOOL_PATH" build --configuration:Debug --target:Clean "$SOLUTION_PATH"
"$VSTOOL_PATH" build --configuration:Debug "$SOLUTION_PATH"