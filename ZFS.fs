module ZFS

open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices
open Infrastructure
open Utils

let elab_file (filepath:string) : unit =
    let elaboratedDir = getDir filepath/"Elaborated"
    Directory.CreateDirectory elaboratedDir |> ignore
    let elaboratedFilePath = elaboratedDir/Path.GetFileName filepath
    try 
        filepath |> ASTUtils.parse_file
                 |> ASTUtils.elab_ast
                 |> ASTUtils.add_main_to_ast
                 |> Ok
    with _ as e ->
        Error e
    |> function | Ok ast -> ASTUtils.write_ast_to_file ast elaboratedFilePath
                            printfn "Wrote elaboratled source to %s" elaboratedFilePath
                | Error e -> printfn "Elaborator error: %A" e
            
(* gets the path to z3 for the system *)
let choose_z3 () : string =
    match Environment.OSVersion.Platform with
    | PlatformID.MacOSX -> "../../packages/zen_z3_osx/output/z3"
    | PlatformID.Unix   -> "../../packages/zen_z3_linux/output/z3"
    | PlatformID.Win32NT -> failwith "No z3 version for Windows"
    | _ -> failwith "OS not supported"

let run_zfs (fn:string) (args : list<string>) : unit =
    let zfs_exe = "../../packages/ZFStar/tools/fstar.exe"
    let z3 = choose_z3()
    Platform.run zfs_exe
        ([ Path.GetFullPath fn
           "--smt";     z3 
           "--prims";   "../../packages/Zulib/output/Zulib/prims.fst"
           "--include"; "../../packages/Zulib/output/Zulib/"
           "--record_hints"
           //"--cache_checked_modules"
           "--no_default_includes" 
         ] @ args)
    |> function | Ok _ -> ()
                | Error e -> printfn "Error: %s" e
    
let verify (fn:string) : unit =
    run_zfs fn []
    
let extract (fn:string) : unit =
    let odir =
        let dir = fn |> Path.GetFullPath 
                     |> Path.GetDirectoryName
                     |> Directory.GetParent
        dir.FullName/"fs"
  
    let module_name = ASTUtils.parse_file fn |> ASTUtils.get_module_name_str
    Directory.CreateDirectory odir |> ignore
    run_zfs fn 
        [ "--codegen"; "FSharp"
          "--odir"; odir 
          "--extract_module"; module_name ]


let compile (fn:string) =
    let odir =
        let dir = fn |> Path.GetFullPath 
                     |> Path.GetDirectoryName
                     |> Directory.GetParent
        dir.FullName/"bin"
    
    Directory.CreateDirectory odir |> ignore
    
    let dll =
        Path.ChangeExtension((odir / Path.GetFileName fn), ".dll")   
    
    let checker = FSharpChecker.Create()
    let checkerArgs = [| "fsc.exe"
                         "--noframework"
                         "--mlcompatibility"
                         "--nowarn:62"
                         "-r"; expandPath "mscorlib.dll"
                         "-r"; expandPath "System.Core.dll"
                         "-r"; expandPath "System.dll"
                         "-r"; expandPath "System.Numerics.dll"
                         "-r"; "Zulib.dll"
                         "-r"; "FSharp.Compatibility.OCaml.dll"
                         "-a"; fn
                         "-o"; dll |]
    let errors, exitCode = 
        checker.Compile(checkerArgs)
        |> Async.RunSynchronously
    if exitCode = 0 then ()
    else 
        let errors = errors |> Array.map string |> String.concat " "
        failwithf "Compile Errors: %s" errors
        