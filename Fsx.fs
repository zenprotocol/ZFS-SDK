(* This module contains functions intended to run and generate .fsx scripts *)
module Fsx

open System
open System.Diagnostics
open Utils

let generate (sourceFilePath : string) : array<string> =
    let contractModuleName = sourceFilePath.Substring(7, sourceFilePath.Length-11)
    [| sprintf "#r \"%s\"" sourceFilePath
       ""
       sprintf "open %s" contractModuleName
       ""
       "let mainFunction = mainFunction"
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
            failwith "failed to run fsx"
    with _ as ex ->
        failwithf "failed to start fsx: \n%s" <| ex.ToString()
    