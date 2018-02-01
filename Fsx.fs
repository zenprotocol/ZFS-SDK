(* This module contains functions intended to run and generate .fsx scripts *)
module Fsx

open System
open System.IO
open System.Diagnostics
open Consensus
open Utils

let generate (sourceFilePath : string) : array<string> =
    let dllPath =
                    "../bin"/Path.ChangeExtension(Path.GetFileName sourceFilePath, ".dll")
    let contractModuleName = contractModuleName sourceFilePath
    let contractHash = getDir sourceFilePath/"Elaborated"/sprintf "%s.fst" contractModuleName
                       |> File.ReadAllText
                       |> Contract.computeHash
                       |> string
    [| sprintf "#r \"%s\"" dllPath
       ""
       "open System.IO"
       "open Consensus"
       "open Consensus.Hash"
       "open Consensus.Types"
       sprintf "open %s" contractModuleName
       ""
       "let f = ZFStar.vectorLength"
       "// Convert the mainFunction from ZF* to F#"
       "let mainFunction = mainFunction |> ZFStar.fstTofsMainFunction"
       ""
       "// Contract Arguments"
       "let txSkeleton = TxSkeleton.empty"
       "let command = \"\""
       "let inputWallet : list<PointedOutput> = []"
       sprintf "let contractHash : Hash = Hash \"%s\"B " contractHash;
       ""
       "// Run the contract on it's arguments and obtain the result"
       "let result = mainFunction txSkeleton contractHash command inputWallet"
       ""
       "printfn \"%A\" result"
       ""
       sprintf "printfn \"Hello World, I am a test for the %s contract!\"" contractModuleName
    |]

let run (fsxFile : string) : unit =
    let fsinteractive() : string = 
        match System.Environment.OSVersion.Platform with
        | PlatformID.Win32NT -> "fsi"
        | PlatformID.MacOSX | PlatformID.Unix -> "fsharpi"
        | platformID -> platformID.ToString()
                        |> failwithf "%s Operating System is Not Supported."
    
    let loadDir="../../.paket/load/"
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
            then ()
            else failwithf "%s" <| p.StandardError.ReadToEnd()
        else 
            failwith "failed to start fsx"
    with _ as ex ->
        failwithf "failed to run fsx: \n%s" <| ex.ToString()
    