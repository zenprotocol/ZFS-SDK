(* This module contains utility functions used pervasively throughout the SDK. *)
module Utils

open System.IO
open Infrastructure

let (/) path1 path2 = Path.Combine(path1, path2)

// returns the path for the folder containing the specified file. 
let getDir : string -> string =
    Path.GetFullPath >> Path.GetDirectoryName
    
let expandPath = (/) Platform.getFrameworkPath