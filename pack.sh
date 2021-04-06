#!/bin/bash

# set the current working directory of the running script to the folder in which the .sh file is located
cd $(dirname "$0")

# pack Flutnet.Interop.iOS
mono /usr/local/bin/nuget.exe pack src/Flutnet.Interop.iOS/Flutnet.Interop.iOS.nuspec -OutputDirectory artifacts/nuget-packages
