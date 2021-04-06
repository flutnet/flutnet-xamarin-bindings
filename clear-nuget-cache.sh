#!/bin/bash

# clear the NuGet package cache, NuGet temp files and its http cache
mono /usr/local/bin/nuget.exe locals all -clear