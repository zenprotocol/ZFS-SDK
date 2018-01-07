
open System.IO

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

// returns the path for the folder containing the specified file. 
let getDir : string -> string =
    Path.GetFullPath >> Path.GetDirectoryName

let (/) path1 path2 = Path.Combine(path1, path2)

let elab_file (filepath:string) : unit =
    let elaboratedDir = getDir filepath/"Elaborated"
    Directory.CreateDirectory elaboratedDir |> ignore
    let elaboratedFilePath = elaboratedDir/Path.GetFileName filepath
    let ast = filepath |> ASTUtils.parse_file
                       |> ASTUtils.elab_ast
                       |> ASTUtils.add_main_to_ast
    ASTUtils.write_ast_to_file ast elaboratedFilePath
    printfn "Wrote elaborated source to %s" elaboratedFilePath
    
[<EntryPoint>]
let main argv =
    match argv with
    | [| "--help" |] -> showUsage()
    | [| sourceFilePath |] -> showUsage()
    | [| sourceFilePath; option |] ->
        match option with
        | "-e" -> elab_file sourceFilePath
        
        | "-v" -> elab_file sourceFilePath
                  let elaboratedDir = getDir sourceFilePath/"Elaborated"
                  let elaboratedFilePath = elaboratedDir/Path.GetFileName sourceFilePath
                  ZFS.verify elaboratedFilePath
        
        | _ -> showUsage()
    | _ -> showUsage()
    
    0 // return an integer exit code
