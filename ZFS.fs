module ZFS

open FStar.Parser.AST
open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices
open Infrastructure
open Utils

let elab_file (filepath:string) =
    log "Elaborating %s ..." (Path.GetFileName(filepath))
    let code = File.ReadAllText filepath
    let moduleName = getModuleName code
    let elaboratedFilePath = (getDir filepath) / moduleName |> changeExtension ".fst"
    let filepath = changeExtension ".source.fst" filepath
    File.WriteAllText(filepath, sprintf "module %s\n%s" moduleName code)

    try 
        filepath |> ASTUtils.parse_file
                 |> ASTUtils.elab_ast
                 |> ASTUtils.add_main_to_ast
                 |> Ok
    with _ as e ->
        Error (sprintf "%A" e)
    |> Result.map (fun ast -> 
        ASTUtils.write_ast_to_file ast elaboratedFilePath
        log "Wrote elaborated source to %s" (Path.GetFileName(elaboratedFilePath))
        elaboratedFilePath)

let run_zfs (fn:string) (args : list<string>) =
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
    
let verify (fn:string) =
    run_zfs fn []
    |> Result.map (fun _ -> 
        log "Verified"
        "")
    
let extract (fn:string) =
    log "Extracting %s ..." (Path.GetFileName(fn))
    let odir = getDir fn
    let module_name = ASTUtils.parse_file fn |> ASTUtils.get_module_name_str
    Directory.CreateDirectory odir |> ignore
    run_zfs fn 
        [ "--codegen"; "FSharp"
          "--odir"; odir 
          "--extract_module"; module_name ]
    |> Result.map (fun _ ->
        let fn = changeExtension ".fs" fn 
        log "Wrote extracted file to %s" (Path.GetFileName(fn))
        fn)

let compile (fn:string) =
    log "Compiling %s ..." (Path.GetFileName(fn))
    let odir = getDir fn 
    Directory.CreateDirectory odir |> ignore
    
    let dll =
        changeExtension ".dll" (odir / Path.GetFileName fn)
    
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
    if exitCode = 0 then
        log "Wrote compiled file to %s" (Path.GetFileName(dll))
        Ok dll
    else 
        let errors = errors |> Array.map string |> String.concat " "
        Error errors
        