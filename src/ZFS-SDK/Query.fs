module Query

open Consensus
open Consensus.Chain
open Consensus.Types
open FSharp.Data
open Infrastructure



let result = Result.result



module ContractId =

    let computeHash version (code:string) =
        let versionBytes = BigEndianBitConverter.uint32ToBytes version
        let codeBytes = System.Text.Encoding.UTF8.GetBytes code
        
        Consensus.Hash.computeMultiple [versionBytes; codeBytes]

    let makeContractId version code =
        ContractId (version, computeHash version code)
    
    let run (filename : string) : Result<string, string> =
        filename
        |> System.IO.File.ReadAllText
        |> makeContractId 0u
        |> sprintf "%A" 
        |> Ok



module Info =

    let getModuleName : Consensus.Hash.Hash -> string =
        Hash.bytes
        >> FsBech32.Base16.encode
        >> (+) "Z"

    let private recordHints rlimit (code:string) : Result<string, string> =
        code
        |> ContractId.computeHash Version0
        |> getModuleName
        |> ZFStar.recordHints rlimit code

    let toJson (contract : ContractV0) : JsonValue =
        JsonValue.Record
            [|
                ( "code"    , JsonValue.String <|        contract.code    )
                ( "rlimit"  , JsonValue.String <| string contract.rlimit  )
                ( "hints"   , JsonValue.String <|        contract.hints   )
                ( "queries" , JsonValue.String <| string contract.queries )
            |]

    let compute (z3rlimit : uint32) (code : string) : Result<ContractV0 , string> =
        result {
            
            return! ZFStar.ModuleDetection.checkNoModuleHeader code
            
            let! hints =
                recordHints z3rlimit code
            
            let! queries =
                ZFStar.totalQueries hints
            
            return {
                code    = code
                rlimit  = z3rlimit
                hints   = hints
                queries = queries
            }
        }
    
    let run (rlimit : uint32) (filename : string) : Result<string, string> =
        filename
        |> System.IO.File.ReadAllText
        |> compute rlimit
        |> Result.map (toJson >> string)



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

    let compute (chain : ChainParameters) (z3rlimit : uint32) (numberOfBlocks:uint32) (code : string) : Result<ActivationCost , string> =
        Infrastructure.Result.result {

            let codeLength = String.length code |> uint64

            let! info = Info.compute z3rlimit code

            let activationFee =
                info.queries * z3rlimit / 100ul |> uint64

            let baseSacrifice =
                chain.sacrificePerByteBlock * codeLength

            let activationSacrifice =
                chain.sacrificePerByteBlock * codeLength * uint64 numberOfBlocks

            return {
                activationFee       = activationFee
                baseSacrifice       = baseSacrifice
                activationSacrifice = activationSacrifice
                numOfBlocks         = numberOfBlocks
                total               = activationFee + activationSacrifice
            }
        }
    
    let run (z3rlimit : uint32) (numberOfBlocks : uint32) (filename : string) : Result<string, string> =
        result {
            
            let code =
                System.IO.File.ReadAllText filename
            
            let! cost =
                compute Consensus.Chain.mainParameters z3rlimit numberOfBlocks code
            
            return (sprintf "%A" cost)
            
        }



let contractId =
    ContractId.run

let activationCost =
    ActivationCost.run

let info =
    Info.run