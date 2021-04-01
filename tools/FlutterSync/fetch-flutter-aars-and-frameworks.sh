#!/bin/bash

# set the current working directory of the running script to the folder in which the .sh file is located
cd $(dirname "$0")

./FlutterSync --TargetDirectory ../../assets/xamarin-native-references
