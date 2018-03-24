#!/bin/sh

# build the sdk
mono .paket/paket.exe restore
msbuild ZFS-SDK.sln /p:Configuration=Release

# ensure Z3 has execute access permissions
chmod +x packages/zen_z3_linux/output/z3
chmod +x packages/zen_z3_osx/output/z3
