open System.IO
open Utils
open ContractTemplate

let usage = """
USAGE: zebra [--z3rlimit <int>] [< option >] <source file>

PARAMS:
    <source file>      The ZF* source file to use

OPTIONS:
    --create                Create a new template contract
    --elaborate, -e         Elaborate the source file and verify
    --verify, -v            Verify the source file
    --extract, -x           Extract the source file
    --compile, -c           Compile from source file
    --pack, -p              Pack the contract to be activated on zen blockchain
    --generate-fsx, -g      Generate a .fsx file to test the contract with
    --run-fsx, -r           Run the given .fsx file, automatically loading Zen dlls.
    --cid                   Compute contract id
    --z3rlimit <int>        Specify the rlimit to z3.
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
let rec main = function
    | [| option; filePath |] ->
        match option with
        | "--create" when Path.GetExtension(filePath) = ".fst" ->
            
            let code = sprintf codeT (filePath.Substring (0, filePath.Length - 4))

            System.IO.File.WriteAllText (filePath, code)

            log "Created %s" filePath

            Ok filePath

        | "-e" | "--elaborate" when expected ".fst" filePath ->
            ZFS.elab_file filePath
            >>= ZFS.verify

        | "-v" | "--verify" when expected ".fst" filePath ->
            ZFS.verify filePath

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

        | "-p" | "--pack" when expected ".fst" filePath ->
            ZFS.pack filePath
        
        | "--cid" when expected ".fst" filePath ->
            ContractId.showContractId filePath

        | _ ->
            Error ""
        |> function
        | Error error ->
            if error <> "" then log "Error: %A" error else showUsage()
            1
        | Ok _ ->
            0
    | [| "--z3rlimit"; z3rlim; option; filePath |] ->
        ZFS.z3rlimit := System.Int32.Parse z3rlim
                        |> Some
        main [|option; filePath|]

    | _ ->
        showUsage()
        0
