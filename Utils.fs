(* This module contains utility functions used pervasively throughout the SDK. *)
module Utils

open System
open System.IO
open Infrastructure
open Consensus
open FsBech32
module String = FSharpx.String

let (/) path1 path2 = Path.Combine(path1, path2)

let (>>=) a b = Result.bind b a

// returns the path for the folder containing the specified file. 
let getDir : string -> string =
    Path.GetFullPath >> Path.GetDirectoryName
    
let expandPath = (/) Platform.getFrameworkPath

// returns a module name from source code by hashing it
let getModuleName =
    Contract.computeHash
    >> Hash.bytes
    >> Base16.encode
    >> (+) "Z"

let changeExtension extension fileName =
    Path.ChangeExtension(fileName, extension)

let log format = 
    printf "SDK:\t"
    printfn format
    
(* gets the path to z3 for the system *)
let choose_z3 () : string =
    match Environment.OSVersion.Platform with
    | PlatformID.MacOSX -> "../../packages/zen_z3_osx/output/z3"
    | PlatformID.Unix   -> "../../packages/zen_z3_linux/output/z3"
    | PlatformID.Win32NT -> failwith "No z3 version for Windows"
    | _ -> failwith "OS not supported"
