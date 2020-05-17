module ZFS

open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices
open Utils

let zfs_exe =
    "fstar.exe"


let elab_file (fst_file:string) : Result<string, string> =
    
    log "Elaborating %s ..." (Path.GetFileName fst_file)
    
    let code =
        File.ReadAllText fst_file
    
    let moduleName =
        ASTUtils.parse_file fst_file
        |> ASTUtils.get_module_name
    
    ensureDirectory <| getDir fst_file / "output"
    
    let elaboratedFilePath =
        getDir fst_file / "output" / moduleName
        |> changeExtension ".fst"
    
    File.WriteAllText (fst_file, code)
    
    let write_elaborated ast =
        ASTUtils.write_ast_to_file elaboratedFilePath ast
        log "Wrote elaborated source to %s" (Path.GetFileName elaboratedFilePath)
        elaboratedFilePath
    
    try
        fst_file |> ASTUtils.parse_file
                 |> ASTUtils.elab_ast
                 |> ASTUtils.add_main_to_ast
                 |> Ok
    
    with _ as e ->
        Error (sprintf "%A" e)
    
    |> Result.map write_elaborated


let private run_zfs (z3rlimit : option<uint32>) (args : list<string>) (fst_file:string) : Result<unit, string> =
    
    let z3 =
        choose_z3()
    
    let rlimit =
        match z3rlimit with
        | Some rlimit ->
            sprintf "--z3rlimit %d" rlimit
        | None ->
            ""
    
    Platform.run zfs_exe
        ([ Path.GetFullPath fst_file
           "--smt"; z3
           "--prims"; "./zulib/prims.fst"
           "--include"; "./zulib/"
           "--record_hints"
           "--use_hints"
           "--use_cached_modules"
           "--no_default_includes"
           rlimit
         ] @ args)


let verify z3rlimit args (fst_file:string) : Result<string, string> =
    
    run_zfs z3rlimit args fst_file
    
    |> Result.map (fun()-> log "Verified"; "")


let extract z3rlimit extra_args (fst_file:string) =
    
    let writeExtracted() =
        
        let fs_file =
            changeExtension ".fs" fst_file
        
        log "Wrote extracted file to %s" (Path.GetFileName fs_file)
        
        fs_file
    
    log "Extracting %s ..." (Path.GetFileName fst_file)
    
    let odir =
        getDir fst_file
    
    let module_name =
        ASTUtils.parse_file fst_file |> ASTUtils.get_module_name
    
    ensureDirectory odir
    
    let args =
        [ "--codegen"; "FSharp"
          "--odir"; odir
          "--extract_module"; module_name
        ] @ extra_args
    
    run_zfs z3rlimit args fst_file
    
    |> Result.map writeExtracted


let pack (fst_file:string) : Result<string, string> =
    
    let moduleName =
        ASTUtils.parse_file fst_file |> ASTUtils.get_module_name
    
    let moduleLine =
        sprintf "module %s" moduleName

    let code =
        let lines =
            File.ReadAllLines fst_file
            |> Array.filter (fun line -> line <> moduleLine)

        String.Join ("\n", lines)

    let fileName =
        sprintf "%s.fst" <| getModuleName code

    File.WriteAllText (fileName, code)

    log "Pakced to %s" fileName

    Ok fileName


let compile (fst_file:string) : Result<string, string> =
    
    log "Compiling %s ..." (Path.GetFileName fst_file)
    
    let odir =
        getDir fst_file
    
    ensureDirectory odir

    let dll =
        changeExtension ".dll" (odir / Path.GetFileName fst_file)

    let checker =
        FSharpChecker.Create()
    
    let checkerArgs =
        [| "fsc.exe"
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
           "-a"; fst_file
           "-o"; dll
        |]
    
    let errors, exitCode =
        checker.Compile checkerArgs
        |> Async.RunSynchronously
    
    if exitCode = 0 then
        
        log "Wrote compiled file to %s" (Path.GetFileName dll)
        
        Ok dll
    
    else
        
        let errors =
            errors
            |> Array.map string
            |> String.concat " "
        
        Error errors


let contractId (fst_file : string) : Result<string, string> =
    
    let code =
        File.ReadAllText fst_file
    
    let cid =
        Extra.ContractId.makeContractId 0u code
    
    printfn "%A" cid
    
    Ok (sprintf "%A" cid)


let activationCost (z3rlimit : uint32) (fst_file : string) (numberOfBlocks : uint32) : Result<string, string> =
    
    let code =
        File.ReadAllText fst_file
    
    let cost =
        Extra.ActivationCost.compute Consensus.Chain.mainParameters z3rlimit numberOfBlocks code
    
    printfn "%A" cost
    
    Ok (sprintf "%A" cost)