(* This module contains functions intended to run and generate .fsx scripts *)
module FSX

open Consensus
open Consensus.Types
open Infrastructure
open Utils


let computeContractHash (version : uint32) (code:string) : Hash.Hash =
    
    let versionBytes =
        BigEndianBitConverter.uint32ToBytes version
    
    let codeBytes =
        System.Text.Encoding.UTF8.GetBytes code

    Hash.computeMultiple [versionBytes; codeBytes]


let generate (module_name : string) (contract_code : string) : string =
    
    let contractHash =
        contract_code
        |> computeContractHash Version0
        |> Hash.toString
    
    let assemblyPath =
        "output" / module_name + ".dll"
    
    ScriptTemplate.generateScriptCode contractHash assemblyPath
