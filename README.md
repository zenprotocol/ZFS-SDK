# ZFS-SDK

## Installation

You will need to have mono installed. Instructions for your OS can be found [here](http://www.mono-project.com/download/).
For Linux, you will need either the `mono-devel` or `mono-complete` package. Do not install mono from your package manager - follow the instructions on the mono website.

### Linux
1. Clone this repo
2. Run the following command. Note that this could take a few minutes.
    ```
    cd ZFS-SDK
    ./paket install
    ./build.sh
    ```

### OSX

1. Clone this repo
2. Run the following command. Note that this could take a few minutes.
    ```
    cd ZFS-SDK
    mono paket install
    ./build.sh
    ```

## Usage
```
    USAGE: ZFS_SDK.exe --help
           ZFS_SDK.exe [< option >] <source file path>

    PARAMS:
        <source file path>      The ZF* source file to use

    OPTIONS:
        --help, -h              Display this list of options
        --elaborate, -e         Elaborate The source File
        --verify, -v            Verify the source file
        --extract, -x           Extract the source file
        --compile, -c           Compile from source file
        --generate-fsx, -g      Generate a .fsx file to test the contract with
        --run-fsx, -r           Run the given .fsx file, automatically loading Zen dlls.
```
