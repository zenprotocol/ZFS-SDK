module ZFS

open System
open System.IO
open Infrastructure
open FSharpx.String

open Microsoft.FSharp.Compiler.SourceCodeServices

let private (/) a b = Path.Combine (a,b)
let private expandPath = (/) Platform.getFrameworkPath

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
           "--prims";   "../../paket-files/gitlab.com/zenprotocol/zenprotocol/src/Zulib/fstar/prims.fst"
           "--include"; "../../paket-files/gitlab.com/zenprotocol/zenprotocol/src/Zulib/fstar"
           "--use_hints"
           "--use_hint_hashes"
           "--record_hints"
           "--cache_checked_modules"
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
                         "-o"; dll
                      |]
    let errors, exitCode = 
        checker.Compile(checkerArgs)
        |> Async.RunSynchronously
    if exitCode = 0 then () 
    else 
        let errors = errors |> Array.map(fun e -> e.ToString()) |> String.concat " "
        failwithf "Compile Errors: %s" errors
        