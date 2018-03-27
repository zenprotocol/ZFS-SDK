open System.IO
open Utils
open ZFS

let usage = """
USAGE: ZFS_SDK.exe [< option >] <source file>

PARAMS:
    <source file>      The ZF* source file to use

OPTIONS:
    --elaborate, -e         Elaborate The source File
    --verify, -v            Verify the source file
    --extract, -x           Extract the source file
    --compile, -c           Compile from source file
    --generate-fsx, -g      Generate a .fsx file to test the contract with
    --run-fsx, -r           Run the given .fsx file, automatically loading Zen dlls.
"""


let showUsage() =
    printfn "%s" usage

let expected extension filePath = 
    if Path.GetExtension(filePath) <> extension then
        log "Expected %s extension" extension
        false
    else if not <| File.Exists filePath then
        log "File not found: %s" filePath
        false
    else
        true
        
[<EntryPoint>]
let main = function
    | [| option; filePath |] ->
        match option with
        | "-e" | "--elaborate" when expected ".fst" filePath -> 
            ZFS.elab_file filePath

        | "-v" | "--verify" when expected ".fst" filePath -> 
            ZFS.elab_file filePath
            >>= ZFS.verify

        | "-x" | "--extract" when expected ".fst" filePath -> 
            ZFS.elab_file filePath
            >>= ZFS.extract 

        | "-c" | "--compile" when expected ".fst" filePath -> 
            ZFS.elab_file filePath
            >>= ZFS.extract 
            >>= ZFS.compile

        | "-g" | "--generate-fsx" when expected ".fst" filePath ->
            Fsx.generate filePath
            
        | "-r" | "--run-fsx" when expected ".fsx" filePath ->
            Fsx.run filePath

        | _ ->
            Error ""
        |> function
        | Error error -> 
            if error <> "" then log "Error: %A" error
            1
        | Ok _ ->
            0
    | _ ->
        showUsage()
        0