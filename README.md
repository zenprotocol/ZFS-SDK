# ZFS-SDK

## Installation

### Linux

1. Clone this repo
2. Run the following command. Note that this could take a few minutes.
    ```
    cd ZFS-SDK
    ./paket install
    ./build.sh
    ```   
## Usage

ZFS_SDK.exe is located in the `/bin/Release/` directory.

USAGE: ZFS_SDK.exe --help
       ZFS_SDK.exe <source file path> [< option >]

PARAMS:
    <source file path>      The ZF* source file to use

OPTIONS:
    --help, -h              Display this list of options
    -e                      Elaborate The source File
    -v                      Verify the source file
    -x                      Extract the source file
    -c                      Compile from source file
    -g                      Generate a .fsx file to test the contract with
