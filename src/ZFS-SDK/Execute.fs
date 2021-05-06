module Execute

open System

open System.Diagnostics
open Utils



module FSX =
    
    let execute script_filename =
        
        let fsinteractive : string =
            match Platform.platform with
            | PlatformID.Win32NT ->
                "fsi"
            | PlatformID.MacOSX | PlatformID.Unix ->
                "fsharpi"
            | _ ->
                Environment.OSVersion.Platform.ToString()
                |> failwithf "%s Operating System is Not Supported."
        
        let working_directory =
            Reflection.Assembly.GetExecutingAssembly().Location
            |> IO.Path.GetDirectoryName
        
        let args : string =
            String.concat " "
                [|
                    sprintf "--lib:%s" working_directory
                    sprintf "--reference:%s" (working_directory / "Zulib.dll")
                    sprintf "--reference:%s" (working_directory / "Consensus.dll")
                    sprintf "--reference:%s" (working_directory / "Infrastructure.dll")
                    sprintf "--reference:%s" (working_directory / "ContractsTestingLib.dll")
                    script_filename
                |]
        
        let pStartInfo =
            Diagnostics.ProcessStartInfo
                ( fsinteractive
                , args
                , RedirectStandardOutput = true
                , RedirectStandardError  = true
                , UseShellExecute        = false
                )
        
        use proc =
            new Diagnostics.Process(StartInfo=pStartInfo)
        
        let outHandler (_sender:obj) (args:Diagnostics.DataReceivedEventArgs) =
            printfn "%s" args.Data
        
        let errHandler (_sender:obj) (args:Diagnostics.DataReceivedEventArgs) =
            eprintfn "%s" args.Data
        
        try
            if proc.Start() then
                proc.ErrorDataReceived.AddHandler <| DataReceivedEventHandler errHandler
                proc.OutputDataReceived.AddHandler <| DataReceivedEventHandler outHandler
                proc.BeginErrorReadLine()
                proc.BeginOutputReadLine()
                proc.WaitForExit()
                if proc.ExitCode = 0 then
                    Ok ""
                else
                    proc.StandardError.ReadToEnd()
                    |> Error
            else
                Error "failed to start fsx"
        with _ as ex ->
            Error (sprintf "failed to run fsx: \n%s" <| ex.ToString())



module ZFStar =
    
    [<Literal>]
    let ZFS_EXE =
        "fstar.exe"
    
    let execute (z3rlimit : option<uint32>) (extra_arguments : list<string>) (fst_file : string) : Result<unit, string> =
        
        let z3 =
            choose_z3()
        
        let rlimit =
            match z3rlimit with
            | Some rlimit ->
                sprintf "--z3rlimit %d" rlimit
            | None ->
                ""
        
        let args =
            [
                System.IO.Path.GetFullPath fst_file
                "--smt"; z3
                "--prims"; "./zulib/prims.fst"
                "--include"; "./zulib/"
                "--record_hints"
                "--use_hints"
                "--use_cached_modules"
                "--no_default_includes"
                rlimit
            ] @ extra_arguments
        
        Platform.run ZFS_EXE args



let fsx : string -> Result<string , string> =
    FSX.execute

let zfstar =
    ZFStar.execute