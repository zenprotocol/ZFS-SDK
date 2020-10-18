module ZFS

open FStar.Parser.AST
open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices
open Infrastructure
open Utils

//let z3rlimit: Ref< option<int> > = ref None

let elab_file (filepath:string) =
    log "Elaborating %s ..." (Path.GetFileName(filepath))
    let code = File.ReadAllText filepath
    let moduleName = ASTUtils.parse_file filepath |> ASTUtils.get_module_name

    ensureDirectory <| (getDir filepath) / "output"

    let elaboratedFilePath = (getDir filepath) / "output" / moduleName |> changeExtension ".fst"
    File.WriteAllText(filepath, code)

    try
        filepath |> ASTUtils.parse_file
                 |> ASTUtils.elab_ast
                 |> ASTUtils.add_main_to_ast
                 |> Ok
    with _ as e ->
        Error (sprintf "%A" e)
    |> Result.map (fun ast ->
        ASTUtils.write_ast_to_file elaboratedFilePath ast
        log "Wrote elaborated source to %s" (Path.GetFileName(elaboratedFilePath))
        elaboratedFilePath)

let run_zfs (z3rlimit : option<int>) (fn : string) (args : list<string>) =
    let zfs_exe = "fstar.exe"
    let z3 = choose_z3()
    let rlimit =
         match z3rlimit with
         | Some rlimit -> sprintf "--z3rlimit %d" rlimit
         | None -> ""
    Platform.run zfs_exe
        ([ Path.GetFullPath fn
           "--smt";     z3
           "--prims";   "./zulib/prims.fst"
           "--include"; "./zulib/"
           "--record_hints"
           "--use_hints"
           "--use_cached_modules"
           "--no_default_includes"
           rlimit
         ] @ args)

let verify (z3rlimit : option<int>) (fn : string) =
    run_zfs z3rlimit fn []
    |> Result.map (fun _ ->
        log "Verified"
        "")

let extract (z3rlimit : option<int>) (fn : string) =
    log "Extracting %s ..." (Path.GetFileName(fn))
    let odir = getDir fn
    let module_name = ASTUtils.parse_file fn |> ASTUtils.get_module_name
    ensureDirectory odir
    run_zfs z3rlimit fn
        [ "--codegen"; "FSharp"
          "--odir"; odir
          "--extract_module"; module_name ]
    |> Result.map (fun _ ->
        let fn = changeExtension ".fs" fn
        log "Wrote extracted file to %s" (Path.GetFileName(fn))
        fn)

let pack (fn:string) =
    let moduleName = ASTUtils.parse_file fn |> ASTUtils.get_module_name
    let moduleLine = sprintf "module %s" moduleName

    let code =
        let lines =
            File.ReadAllLines fn
            |> Array.filter (fun line -> line <> moduleLine)

        String.Join ("\n",lines)

    let fileName = sprintf "%s.fst" <| getModuleName code

    File.WriteAllText (fileName, code)

    log "Pakced to %s" fileName

    Ok fileName

let compile (fn:string) =
    log "Compiling %s ..." (Path.GetFileName(fn))
    let odir = getDir fn
    ensureDirectory odir

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
                         "-r"; "BouncyCastle.Crypto.dll"
                         "-r"; "FSharpx.Collections.dll"
                         "-r"; "FsBech32.dll" 
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
