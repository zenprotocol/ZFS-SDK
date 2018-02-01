
open System.IO
open Utils

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
    | [| "-h" |] 
    | [| "--help" |]-> 
        showUsage()
    
    | [| _ |] -> 
        showUsage()
    
    | [| option; sourceFilePath|] ->
        match option with
        | "-e" | "--elaborate" -> 
            ZFS.elab_file sourceFilePath
        
        | "-v" | "--verify" -> 
            ZFS.elab_file sourceFilePath
            let elaboratedDir = getDir sourceFilePath/"Elaborated"
            let elaboratedFilePath = elaboratedDir/Path.GetFileName sourceFilePath
            ZFS.verify elaboratedFilePath
        
        | "-x" | "--extract" -> 
            ZFS.elab_file sourceFilePath       
            let elaboratedDir = getDir sourceFilePath/"Elaborated"
            let elaboratedFilePath = elaboratedDir/Path.GetFileName sourceFilePath
            ZFS.extract elaboratedFilePath
        
        | "-c" | "--compile" -> 
            main [| "-x"; sourceFilePath |] |> ignore
            let outputFile = Path.ChangeExtension(Path.GetFileName sourceFilePath, ".fs")
            let outputFilePath = getDir sourceFilePath/"fs"/outputFile
            ZFS.compile outputFilePath
            
        | "-g" | "--generate-fsx" ->
            let dllPath =
                let dllFile = Path.ChangeExtension(Path.GetFileName sourceFilePath, ".dll")
                "../bin"/dllFile
            let template = Fsx.generate dllPath
            let fsxFile = getDir sourceFilePath/"fs"/"TestContract.fsx"
            File.WriteAllLines(fsxFile, template)
            printfn "Generated %s" fsxFile
            
        | "-r" | "--run-fsx" ->
            Fsx.run sourceFilePath
        
        | _ ->
            showUsage()
    | _ ->
        showUsage()
    
    0 // return an integer exit code
