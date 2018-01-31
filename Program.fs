
open System
open System.IO
open System.Text
open System.Diagnostics
open Microsoft.FSharp.Compiler.Interactive.Shell
open Zen.TxSkeleton
open Zen.Types.Realized

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
    printfn "Wrote elaboratled source to %s" elaboratedFilePath

let run_fsx (fsxFile : string) : unit =
    let fsinteractive() : string = 
        match System.Environment.OSVersion.Platform with
        | PlatformID.Win32NT -> "fsi"
        | PlatformID.MacOSX | PlatformID.Unix -> "fsharpi"
        | platformID -> platformID.ToString()
                        |> failwithf "%s Operating System is Not Supported."
    
    let loadDir="../../.paket/load/"
    let args : string =
        String.concat " " 
                      [| sprintf "--load:%s" (loadDir/"Zulib.fsx")
                         sprintf "--load:%s" (loadDir/"Consensus.fsx")
                         sprintf "--load:%s" (loadDir/"Infrastructure.fsx")
                         fsxFile |]
    let pStartInfo = ProcessStartInfo(  
                        fsinteractive(), 
                        args, 
                        RedirectStandardOutput=true,
                        RedirectStandardError=true,
                        UseShellExecute=false
                        )
    use p = new Process(StartInfo=pStartInfo)
    try
        if p.Start() then
            p.WaitForExit()
            printfn "%s" <| p.StandardOutput.ReadToEnd()
            if p.ExitCode = 0 
            then ()
            else failwithf "%s" <| p.StandardError.ReadToEnd()
        else 
            failwith "failed to run fsx"
    with _ as ex ->
        failwithf "failed to start fsx: \n%s" <| ex.ToString()
    
let usage =
    """
    USAGE: ZFS_SDK.exe --help
           ZFS_SDK.exe [< option >] <source file path>
    
    PARAMS:
        <source file path>      The ZF* source file to use
    
    OPTIONS:
        --help, -h              Display this list of options
        --elaborate, -e         Elaborate The source File
        --verify, -v            Verify the source file
        --extract, -x           Extract the source file
        --compile, -c           Compile from source file
        --generate-fsx, -g      Generate a .fsx file to test the contract with
        --run-fsx, -r           Run the given .fsx file, automatically loading Zen dlls.
    """
let showUsage() : unit =
    printfn "%s" usage
   
[<EntryPoint>]
let rec main argv =
    match argv with
    | [| "--help" |] -> 
        showUsage()
    
    | [| _ |] -> 
        showUsage()
    
    | [| option; sourceFilePath|] ->
        match option with
        | "--elaborate" | "-e" -> 
            elab_file sourceFilePath
        
        | "--verify" | "-v" -> 
            elab_file sourceFilePath
            let elaboratedDir = getDir sourceFilePath/"Elaborated"
            let elaboratedFilePath = elaboratedDir/Path.GetFileName sourceFilePath
            ZFS.verify elaboratedFilePath
        
        | "--extract" | "-x" -> 
            elab_file sourceFilePath       
            let elaboratedDir = getDir sourceFilePath/"Elaborated"
            let elaboratedFilePath = elaboratedDir/Path.GetFileName sourceFilePath
            ZFS.extract elaboratedFilePath
        
        | "--compile" | "-c" -> 
            main [| "-x"; sourceFilePath |] |> ignore
            let outputFile = Path.ChangeExtension(Path.GetFileName sourceFilePath, ".fs")
            let outputFilePath = getDir sourceFilePath/"fs"/outputFile
            ZFS.compile outputFilePath
            
        | "--generate-fsx" | "-g" ->
            let dllPath =
                let dllFile = Path.ChangeExtension(Path.GetFileName sourceFilePath, ".dll")
                "../bin"/dllFile
            let template = TemplateFSX.generate dllPath
            let fsxFile = getDir sourceFilePath/"fs"/"TestContract.fsx"
            File.WriteAllLines(fsxFile, template)
            printfn "Generated %s" fsxFile
            
        | "--run-fsx" | "-r" ->
            run_fsx sourceFilePath
        
        | _ ->
            showUsage()
    | _ ->
        showUsage()
    
    0 // return an integer exit code
