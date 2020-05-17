module Extra

open Consensus
open Consensus.Chain
open Consensus.Types
open Infrastructure

module ContractId =
    
    let computeHash version (code:string) =
        let versionBytes = BigEndianBitConverter.uint32ToBytes version
        let codeBytes = System.Text.Encoding.UTF8.GetBytes code

        Consensus.Hash.computeMultiple [versionBytes; codeBytes]

    let makeContractId version code =
        ContractId (version, computeHash version code)


module ActivationCost =
    
    type ActivationCost =
        {
            activationFee : uint64
            baseSacrifice : uint64
            activationSacrifice : uint64
            numOfBlocks : uint32
            total : uint64
        }
    
    let private getModuleName : Hash.Hash -> string =
        Hash.bytes
        >> FsBech32.Base16.encode
        >> (+) "Z"
    
    let recordHints (code:string) : Result<string, string> =
        code
        |> ContractId.computeHash Version0
        |> getModuleName
        |> ZFStar.recordHints code
    
    let compute (chain : ChainParameters) rlimit (numberOfBlocks:uint32) code =
        let contractId = ContractId.makeContractId Version0 code

        let  hints = Measure.measure
                        (sprintf "recording hints for contract %A" contractId)
                        (lazy(recordHints code))
                        |> Result.get
        
        let queries = ZFStar.totalQueries hints |> Result.get

        let codeLength = String.length code |> uint64
        
        let activationFee = queries * rlimit / 100ul |> uint64
        let baseSacrifice = chain.sacrificePerByteBlock * codeLength
        let activationSacrifice = chain.sacrificePerByteBlock * codeLength * (uint64 numberOfBlocks)

        {
            activationFee = activationFee
            baseSacrifice = baseSacrifice
            activationSacrifice = activationSacrifice
            numOfBlocks = numberOfBlocks
            total = activationFee + activationSacrifice 
        }