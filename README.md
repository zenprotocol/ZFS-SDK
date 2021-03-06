---
description: >-
  Learn how to use the ZP Smart Contract SDK to write, compile and test smart
  contracts in the ZF* language.
---

# Smart Contracts SDK

Zen-SDK Repo -&gt; [https://github.com/zenprotocol/ZFS-SDK](https://github.com/zenprotocol/ZFS-SDK)

## Installation

You will need to have mono installed. Instructions for your OS can be found [here](http://www.mono-project.com/download/). For Linux, you will need either the `mono-devel` or `mono-complete` package. Do not install mono from your package manager - follow the instructions on the mono website.

### Linux

* Clone this repo
* Run the following commands. Note that this could take a few minutes.
* `cd ZFS-SDK`
* `./paket restore`
* `./build.sh`

### OSX

* Clone this repo
* Run the following commands. Note that this could take a few minutes.
* `cd ZFS-SDK`
* `mono paket restore`
* `./build.sh`

## Usage

```bash
USAGE: zebra [--help] [<subcommand> [<options>]]

SUBCOMMANDS:

    create <options>      create a new template contract
    elaborate, e <options>
                          elaborate the source File and verify
    verify, v <options>   verify the source file
    extract, x <options>  extract the source file
    compile, c <options>  compile from source file
    pack, p <options>     pack the contract to be activated on zen blockchain
    generate-fsx, g <options>
                          generate a .fsx file to test the contract with
    run-fsx, r <options>  run the given .fsx file, automatically loading Zen dlls
    contractid, cid <options>
                          compute contract ID
    acost, ac <options>   compute activation cost
    info, i <options>     get contract information

    Use 'zebra <subcommand> --help' for additional information.

OPTIONS:

    fields                list possible info fields
    --help                display this list of options.
```

### create

```bash
USAGE: zebra create [--help] <filename>

FILENAME:

    <filename>            file name of the generated contract

OPTIONS:

    --help                display this list of options.
```

### elaborate

```bash
USAGE: zebra elaborate [--help] [--z3rlimit <rlimit>] [--log-types] <filename>

FILENAME:

    <filename>            file name of the contract to elaborate

OPTIONS:

    --z3rlimit, -z <rlimit>
                          Z3 rlimit
    --log-types, -t       log types
    --help                display this list of options.
```

### verify

```bash
USAGE: zebra verify [--help] [--z3rlimit <rlimit>] [--log-types] <filename>

FILENAME:

    <filename>            file name of the contract to verify

OPTIONS:

    --z3rlimit, -z <rlimit>
                          Z3 rlimit
    --log-types, -t       log types
    --help                display this list of options.
```

### extract

```bash
USAGE: zebra extract [--help] [--z3rlimit <rlimit>] [--log-types] <filename>

FILENAME:

    <filename>            file name of the contract to extract

OPTIONS:

    --z3rlimit, -z <rlimit>
                          Z3 rlimit
    --log-types, -t       log types
    --help                display this list of options.
```

### compile

```bash
USAGE: zebra compile [--help] [--z3rlimit <rlimit>] [--log-types] <filename>

FILENAME:

    <filename>            file name of the contract to compile

OPTIONS:

    --z3rlimit, -z <rlimit>
                          Z3 rlimit
    --log-types, -t       log types
    --help                display this list of options.
```

### pack

```bash
USAGE: zebra pack [--help] <filename>

FILENAME:

    <filename>            file name of the contract to pack

OPTIONS:

    --help                display this list of options.
```

### generate-fsx

```bash
USAGE: zebra generate-fsx [--help] <filename>

FILENAME:

    <filename>            file name of the source contract to generate FSX script from

OPTIONS:

    --help                display this list of options.
```

### run-fsx

```bash
USAGE: zebra run-fsx [--help] <filename>

FILENAME:

    <filename>            file name of the FSX script

OPTIONS:

    --help                display this list of options.
```

### contractid

```bash
USAGE: zebra contractid [--help] <filename>

FILENAME:

    <filename>            file name of the contract

OPTIONS:

    --help                display this list of options.
```

### acost

```bash
USAGE: zebra acost [--help] [--numofblocks <uint>] [--z3rlimit <rlimit>] <filename>

FILENAME:

    <filename>            file name of the contract

OPTIONS:

    --numofblocks, -n <uint>
                          number of blocks
    --z3rlimit, -z <rlimit>
                          Z3 rlimit
    --help                display this list of options.
```

### info

```bash
USAGE: zebra info [--help] [--z3rlimit <rlimit>] <filename>

FILENAME:

    <filename>            file name of the contract

OPTIONS:

    --z3rlimit, -z <rlimit>
                          Z3 rlimit
    --field, -f <string>  return info about a specific field
    --help                display this list of options.
```

