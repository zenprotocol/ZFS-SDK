module ZFS

(* gets the path to z3 for the system *)
let choose_z3 () : string =
    match System.Environment.OSVersion.Platform with
    | System.PlatformID.MacOSX -> "../../packages/zen_z3_osx/output/z3"
    | System.PlatformID.Unix   -> "../../packages/zen_z3_linux/output/z3"
    | System.PlatformID.Win32NT -> failwith "No z3 version for Windows"
    | _ -> failwith "OS not supported"

let run_zfs (fn:string) (args : list<string>) : unit =
    let zfs_exe = "../../packages/ZFStar/tools/fstar.exe"
    let z3 = choose_z3()
    Infrastructure.Platform.run zfs_exe
        ([ System.IO.Path.GetFullPath fn
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
    
let extract (fn:string) =
    ()
