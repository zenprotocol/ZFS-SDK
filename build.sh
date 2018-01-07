#!/bin/sh

exit_code=$?
  if [ $exit_code -ne 0 ]; then
    exit $exit_code
  fi

# build Zulib
cd paket-files/gitlab.com/zenprotocol/zenprotocol/src/Zulib
./build.sh

# build zenprotocol
cd .. 
msbuild zenprotocol.sln

# build the sdk
cd ../../../../..
mono .paket/paket.exe restore
# z3 needs to be executable
chmod +x packages/zen_z3_linux/output/z3
# ZF* needs to be executable
chmod +x packages/ZFStar/tools/fstar.exe
msbuild ZFS-SDK.sln
