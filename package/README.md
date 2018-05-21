# ZFS-SDK

## Installation

Prerequisites:
* NodeJS
* Mono

Run the following commands in the Terminal / Command Prompt:

```
npm config set @zen:registry https://www.myget.org/F/zenprotocol/npm/
npm install @zen/zebra -g
```

## Usage
Run `zebra --create ContractName.fst` to generate a template contract.

Run `zebra -v ContractName.fst` to verify that the code is correct.

Run `zebra -e ContractName.fst` to elaborate the file, this will also check the cost of the contract and will output the correct cost in case of mismatch.

Run `zebra -c ContractName.fst` to compile the contract to .Net assembly.

Run `zebra -g ContractName.fst` to generate fsx file to test the contract.

Run `zebra -r ContractName.fsx` to run the test file and test the contract.

Run `zebra -p ContractName.fst` to pack the contract for activation on Zen Protocol blockchain. This will create a contract file with the hash of the contract as the file name.
