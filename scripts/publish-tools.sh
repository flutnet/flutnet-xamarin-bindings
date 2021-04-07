#!/bin/bash

# set the current working directory of the running script to the folder in which the .sh file is located
cd $(dirname "$0")

# publish executable for macOS
dotnet publish ../toolsSrc/FlutterSync/FlutterSync.csproj -c Release -f netcoreapp3.1 -o ../tools/FlutterSync -p:PublishSingleFile=true -r osx-x64 --no-self-contained

# publish exeutable for Windows
dotnet publish ../toolsSrc/FlutterSync/FlutterSync.csproj -c Release -f netcoreapp3.1 -o ../tools/FlutterSync -p:PublishSingleFile=true -r win-x64 --no-self-contained
