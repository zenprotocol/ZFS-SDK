

let usage =
    """
    USAGE: ZFS_SDK.exe --help
           ZFS_SDK.exe <source file path> [< option >]
    
    PARAMS:
        <source file path>      The ZF* source file to use
    
    OPTIONS:
        --help, -h              display this list of options
    """
let showUsage() : unit =
    printfn "%s" usage

[<EntryPoint>]
let main argv =
    match argv with
    | [| "--help" |] -> showUsage()
    | [| sourceFilePath |] -> showUsage()
    | [| sourceFilePath; "-e" |] -> ()        
    | _ -> showUsage()
    
    0 // return an integer exit code
