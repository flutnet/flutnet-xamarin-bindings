#!/bin/bash

# set the current working directory of the running script to the folder in which the .sh file is located
cd $(dirname "$0")

cd flutter_module

#
flutter pub get

#
flutter build ios-framework --no-profile