(* This module contains utility functions used pervasively throughout the SDK. *)
module Utils

open System
open System.IO
open Infrastructure
open Consensus
open FsBech32
open Consensus.Types

module String = FSharpx.String

let (/) (path1 : string) (path2 : string) : string =
    Path.Combine(path1, path2)

let (>>=) (m : Result<'a , 'err>) (f : 'a -> Result<'b , 'err>) : Result<'b , 'err> =
    Result.bind f m

// returns the path for the folder containing the specified file.
let getDir : string -> string =
    Path.GetFullPath >> Path.GetDirectoryName

let expandPath : string -> string =
    (/) Platform.getFrameworkPath

// returns a module name from source code by hashing it
let getPackedModuleName : string -> string =
    Contract.makeContractId Version0
    >> ContractId.contractHash
    >> Hash.bytes
    >> Base16.encode
    >> (+) "Z"

let changeExtension (extension : string) (filename : string) : string =
    Path.ChangeExtension(filename , extension)

let log (format : Printf.TextWriterFormat<'f>) : 'f =
    printf "SDK:\t"
    printfn format

(* gets the path to z3 for the system *)
let choose_z3 () : string =
    match Platform.platform with
    | PlatformID.MacOSX  -> "z3-osx"
    | PlatformID.Unix    -> "z3-linux"
    | PlatformID.Win32NT -> "z3.exe"
    | _                  -> failwith "OS not supported"

let createDirectory (path : string) : unit =
    if not <| Directory.Exists path then
        Directory.CreateDirectory path |> ignore
