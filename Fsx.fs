(* This module contains functions intended to run and generate .fsx scripts *)
module Fsx

open System
open System.IO
open System.Diagnostics
open System.Reflection
open Consensus
open Infrastructure
open Utils
open FsBech32

let generate fileName = 
    let code = File.ReadAllText fileName
    let contractHash = code
                       |> Contract.computeHash
                       |> Hash.bytes
                       |> Base16.encode

    let tpl = sprintf """
open Infrastructure
open Consensus
open Hash
open Types
open Zen.Types.Data

let load = Contract.load "." 0ul 0ul ""

// Contract Arguments
let command = ""
let data = Empty
ler returnAddress = None
let wallet : list<PointedOutput> = []
let tx = TxSkeleton.empty

// Contract Entrypoint
let entrypoint (contract : Contract.T) =
    printfn "cost fn result: %%A" (contract.costFn command data returnAddress wallet tx)

    match contract.fn contract.hash command data returnAddress wallet tx with
    | Ok (tx, message) ->
        printfn "main fn result:\n tx: %%A\n message: %%A" tx message
    | Error error ->
        printfn "main fn error: %%A" error

Hash.fromString "%s"
|> Result.bind load
|> Result.map entrypoint
|> Result.mapError (printfn "Error encountered: %%A")
|> ignore """  contractHash

    let fsxFile = changeExtension ".fsx" fileName
    File.WriteAllText (fsxFile, tpl)
    printfn "Generated. to run:\n/ZFS_SDK.exe -r %s" fsxFile
    Ok ""
    
let run (fsxFile : string) =

    let fsinteractive() : string = 
        match Platform.platform with
        | PlatformID.Win32NT -> "fsi"
        | PlatformID.MacOSX | PlatformID.Unix -> "fsharpi"
        | _ -> Environment.OSVersion.Platform.ToString()
               |> failwithf "%s Operating System is Not Supported."
    
    let workDir = System.Reflection.Assembly.GetExecutingAssembly().Location
                  |> Path.GetDirectoryName
                  
    let loadDir= workDir / "../../.paket/load/"
    let args : string =
        String.concat " " 
                      [| sprintf "--load:%s" (loadDir/"Zulib.fsx")
                         sprintf "--load:%s" (loadDir/"Consensus.fsx")
                         sprintf "--load:%s" (loadDir/"Infrastructure.fsx")
                         fsxFile |]
    let pStartInfo = ProcessStartInfo(  
                        fsinteractive(), 
                        args, 
                        RedirectStandardOutput=true,
                        RedirectStandardError=true,
                        UseShellExecute=false
                        )
    use p = new Process(StartInfo=pStartInfo)
    try
        if p.Start() then
            p.WaitForExit()
            printfn "%s" <| p.StandardOutput.ReadToEnd()
            if p.ExitCode = 0 
            then Ok ""
            else Error (sprintf "%s" <| p.StandardError.ReadToEnd())
        else 
            Error "failed to start fsx"
    with _ as ex ->
        Error (sprintf "failed to run fsx: \n%s" <| ex.ToString())