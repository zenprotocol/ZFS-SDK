#!/bin/sh

rm -r Release
cd ../
rm -r src/ZFS-SDK/bin/Release
msbuild src /property:Configuration=Release
cd package
cp -r ../src/ZFS-SDK/bin/Release .

touch ./Release/.npmignore
npm pack .
