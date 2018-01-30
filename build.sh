#!/bin/sh

# build the sdk
mono .paket/paket.exe restore
msbuild ZFS-SDK.sln /p:Configuration=Release
