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
        result {
            let code =
                filename
                |> System.IO.File.ReadAllText
            
            return! ZFStar.ModuleDetection.checkNoModuleHeader code
            
            return!
                code
                |> makeContractId 0u
                |> sprintf "%A" 
                |> Ok
        }



module Info =
    
    let invalidFieldError : Printf.StringFormat<string -> string> =
        "invalid field: %s"
    
    let private getField (field : string) : Result<System.Reflection.PropertyInfo , string> =
        typeof<ContractV0>.GetProperties()
        |> Array.tryFind (fun item -> item.Name = field)
        |> Result.ofOption (sprintf invalidFieldError field)
    
    let private getFieldValue (field : System.Reflection.PropertyInfo) (info : ContractV0) : Result<string , string> =
        string (field.GetValue info)
        |> Ok
    
    let listFields() : Result<string , string> =
        typeof<ContractV0>.GetProperties()
        |> Array.map (fun item -> item.Name)
        |> Array.toList
        |> String.concat "\n"
        |> Ok

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
    
    let run (rlimit : uint32) (field : string option) (filename : string) : Result<string, string> =
        result {
            match field with

            | Some field ->
                
                let! field =
                    getField field
                
                let! info =
                    filename
                    |> System.IO.File.ReadAllText
                    |> compute rlimit
                
                return! getFieldValue field info

            | None ->
    
                let! info =
                    filename
                    |> System.IO.File.ReadAllText
                    |> compute rlimit
                
                return string (toJson info) 
        }



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