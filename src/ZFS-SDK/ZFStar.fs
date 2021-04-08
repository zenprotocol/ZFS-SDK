module ZFStar

open System
open System.IO
open Microsoft.FSharp.Compiler.SourceCodeServices

open Utils



type AST = ASTUtils.AST



[<Literal>]
let ZFS_EXE =
    "fstar.exe"

[<Literal>]
let OUTPUT_DIR =
    "output"


let result =
    Infrastructure.Result.result


let choose_z3 () : string =
    match Platform.platform with
    | PlatformID.MacOSX  -> "z3-osx"
    | PlatformID.Unix    -> "z3-linux"
    | PlatformID.Win32NT -> "z3.exe"
    | _                  -> failwith "OS not supported"


let defaultArgs fst_file z3 rlimit =
    [
        Path.GetFullPath fst_file
        "--smt"; z3
        "--prims"; "./zulib/prims.fst"
        "--include"; "./zulib/"
        "--record_hints"
        "--use_hints"
        "--use_cached_modules"
        "--no_default_includes"
        rlimit
    ]


let run_zfs (z3rlimit : option<uint32>) (args : list<string>) (fst_file : string) : Result<unit, string> =
    
    let z3 =
        choose_z3()
    
    let rlimit =
        match z3rlimit with
        | Some rlimit ->
            sprintf "--z3rlimit %d" rlimit
        | None ->
            ""
    
    Platform.run ZFS_EXE (defaultArgs fst_file z3 rlimit @ args)


let private outputDir (filename : string) : string =
    getDir filename / OUTPUT_DIR


let createOutputDir (filename : string) : string =
    let odir = outputDir filename
    createDirectory odir; odir



module ModuleDetection =
    
    open System.Text.RegularExpressions
    
    exception BreakException of int
    
    let MODULE_PATTERN : Printf.StringFormat<string -> string> =
        "^.*module\s+%s\s*$"
    
    let HEADER_PATTERN : string =
        "^.*module\s+\w+\s*$"
    
    let hasModule (module_name : string) (line : string) : bool =
        Regex.IsMatch( input = line , pattern = sprintf MODULE_PATTERN module_name )
    
    let getModuleLineIndex (module_name : string) (lines : string[]) : int =
        try
            for i in 0 .. lines.Length - 1 do
                if hasModule module_name lines.[i] then
                    raise (BreakException i)    // Sorry, that's the only nice way to break a loop
            -1
        with BreakException i ->
            i
    
    let hasModuleHeader (code : string) : bool =
        Regex.IsMatch( input = code , pattern = HEADER_PATTERN , options = RegexOptions.Multiline )
    
    let checkModuleHeader (code : string) : Result<unit , string> =
        if hasModuleHeader code then
            Ok()
        else
            Error "The contract must have a module header"
    
    let checkNoModuleHeader (code : string) : Result<unit , string> =
        if hasModuleHeader code then
            Error "The contract shouldn't have a module header"
        else
            Ok()



module private AST =
    
    let parse (filename : string) : Result<AST , string> =
        try
            ASTUtils.parse_file filename
            |> Ok
        with ex ->
            sprintf "%A" ex
            |> Error
    
    let write (filename : string) (ast : AST) : Result<unit , string> =
        try
            ASTUtils.write_ast_to_file filename ast
            |> Ok
        with ex ->
            sprintf "%A" ex
            |> Error
    
    let elaborate (ast : AST) : Result<AST , string> =
        try
            ast
            |> ASTUtils.elab_ast
            |> ASTUtils.add_main_to_ast
            |> Ok
        with ex ->
            sprintf "%A" ex
            |> Error



module Elaborate =
    
    let run (filename : string) : Result<string , string> =
        
        result {
            
            let code =
                System.IO.File.ReadAllText filename
            
            return! ModuleDetection.checkModuleHeader code
            
            log "Parsing %s ..." (Path.GetFileName filename)
            
            let! ast =
                AST.parse filename
            
            log "Elaborating %s ..." (Path.GetFileName filename)
            
            let moduleName =
                ASTUtils.get_module_name ast
            
            let elaborated_filename =
                createOutputDir filename / moduleName + ".fst"
            
            let! elaborated_ast =
                AST.elaborate ast
            
            return! AST.write elaborated_filename elaborated_ast
            
            log "Wrote elaborated source to %s" (Path.GetFileName elaborated_filename)
            
            return elaborated_filename
        
        }



module Verify =
    
    let run (z3rlimit : uint32 option) (args : string list) (filename : string) : Result<string, string> =
        
        result {
            
            let code =
                System.IO.File.ReadAllText filename
            
            return! ModuleDetection.checkModuleHeader code
            
            return! run_zfs z3rlimit args filename
            
            log "Verified"
            
            return filename
        
        }



module Extract =
    
    let extract_args (output_dir : string) (module_name : string) : string list =
        [
            "--codegen"; "FSharp"
            "--odir"; output_dir
            "--extract_module"; module_name
        ]
    
    let run (z3rlimit : uint32 option) (extra_args : string list) (filename : string) : Result<string , string> =
        
        result {
            
            log "Extracting %s ..." (Path.GetFileName filename)
            
            let! ast = 
                AST.parse filename
            
            let module_name =
                ASTUtils.get_module_name ast
            
            // Assumed to be run after elaboration, so the file is already in the output dir
            let output_dir =
                getDir filename
            
            return! run_zfs z3rlimit (extract_args output_dir module_name @ extra_args) filename
                
            let fs_filename =
                changeExtension ".fs" filename
            
            log "Wrote extracted file to %s" (Path.GetFileName fs_filename)
            
            return fs_filename
        }



module Compile =
    
    let checkerArgs filename dll_filename =
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
           "-a"; filename
           "-o"; dll_filename
        |]
    
    let run (filename : string) : Result<string , string> =
        
        log "Compiling %s ..." (Path.GetFileName filename)
        
        let dll_filename =
            changeExtension ".dll" filename
        
        let errors, exitCode =
            checkerArgs filename dll_filename
            |> FSharpChecker.Create().Compile
            |> Async.RunSynchronously
        
        if exitCode = 0 then
            
            log "Wrote compiled file to %s" (Path.GetFileName dll_filename)
            
            Ok dll_filename
        
        else
            
            errors
            |> Array.map string
            |> String.concat " "
            |> Error



module Pack =
    
    let run (filename : string) : Result<string , string> =
        result {
            
            let code =
                System.IO.File.ReadAllText filename
            
            return! ModuleDetection.checkModuleHeader code
            
            let! ast =
                AST.parse filename
            
            let module_name =
                ASTUtils.get_module_name ast
            
            let lines =
                File.ReadAllLines filename
            
            let module_index =
                ModuleDetection.getModuleLineIndex module_name lines
            
            let lines_without_module =
                if module_index >= 0 then
                    let pre =
                        Array.take module_index lines
                    let post =
                        Array.skip (module_index + 1) lines
                    Array.append pre post
                else
                    lines
            
            let code =
                String.Join ("\n", lines_without_module)
            
            let packed_filename =
                getPackedModuleName code + ".fst" 
            
            File.WriteAllText (packed_filename, code)
            
            log "Packed to %s" packed_filename
            
            return packed_filename
        }



let elaborate =
    Elaborate.run

let verify =
    Verify.run

let extract =
    Extract.run

let compile =
    Compile.run

let pack =
    Pack.run

