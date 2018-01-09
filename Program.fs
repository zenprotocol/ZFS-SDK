
open System.IO
open System.Text
open Microsoft.FSharp.Compiler.Interactive.Shell

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
    printfn "Wrote elaborated source to %s" elaboratedFilePath

let run_fsx (fsxFile : string) : Zen.Types.Extracted.transactionSkeleton =
    let inStream = new StringReader("")
    let defaultArgs = [|"fsi.exe";"--noninteractive";"--nologo";"--gui-" |]
    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let fsiSession = FsiEvaluationSession.Create(fsiConfig, defaultArgs, inStream, stdout, stderr) 
    let fsx = File.ReadAllText fsxFile
    match fsiSession.EvalExpression fsx with
    | None -> 
        failwithf "No result when running %s" fsxFile
    | Some value -> 
        value.ReflectionValue |> unbox<Consensus.TxSkeleton.TxSkeleton>
                              |> Consensus.ZFStar.convertInput
    
let usage =
    """
    USAGE: ZFS_SDK.exe --help
           ZFS_SDK.exe <source file path> [< option >]
    
    PARAMS:
        <source file path>      The ZF* source file to use
    
    OPTIONS:
        --help, -h              Display this list of options
        -e                      Elaborate The source File
        -v                      Verify the source file
        -x                      Extract the source file
        -c                      Compile from source file
        -r <input file>         Run the contract on the input file.
    """
let showUsage() : unit =
    printfn "%s" usage
   
[<EntryPoint>]
let rec main argv =
    match argv with
    | [| "--help" |] -> 
        showUsage()
    
    | [| sourceFilePath |] -> 
        showUsage()
    
    | [| sourceFilePath; option |] ->
        match option with
        | "-e" -> 
            elab_file sourceFilePath
        
        | "-v" -> 
            elab_file sourceFilePath
            let elaboratedDir = getDir sourceFilePath/"Elaborated"
            let elaboratedFilePath = elaboratedDir/Path.GetFileName sourceFilePath
            ZFS.verify elaboratedFilePath
        
        | "-x" -> 
            elab_file sourceFilePath       
            let elaboratedDir = getDir sourceFilePath/"Elaborated"
            let elaboratedFilePath = elaboratedDir/Path.GetFileName sourceFilePath
            ZFS.extract elaboratedFilePath
        
        | "-c" -> 
            main [| sourceFilePath; "-x" |] |> ignore
            let odir =
                let dir = sourceFilePath |> Path.GetFullPath 
                                         |> Path.GetDirectoryName
                dir/"Elaborated"/"Output"
            let outputFile = Path.ChangeExtension(Path.GetFileName sourceFilePath, ".fs")
            let outputFilePath = odir/outputFile
            ZFS.compile outputFilePath
            
        | _ -> showUsage()
    
    | [| sourceFilePath; "-r"; inputFilePath |] ->
        main [|sourceFilePath; "-c" |] |> ignore
        let odir =
                        let dir = sourceFilePath |> Path.GetFullPath 
                                                 |> Path.GetDirectoryName
                        dir/"Elaborated"/"Output"
        let dll = 
            let dllFile = Path.ChangeExtension(Path.GetFileName sourceFilePath, ".dll")
            odir / dllFile
       
        let contractHash =
            sourceFilePath |> File.ReadAllText 
                           |> System.Text.Encoding.UTF8.GetBytes
                           |> Consensus.Hash.compute
        
        let contractAssembly = dll |> System.Reflection.Assembly.LoadFrom
        let contractMainFunction = contractAssembly.GetModules().[0]
                                                   .GetMethod("mainFunction")
                                                   .Invoke(null, [||])
                                                   :?> Zen.Types.Extracted.mainFunction
        let contractCostFunction, contractMainFunction =
            match contractMainFunction with 
            | Zen.Types.Extracted.MainFunc (cf, mf) -> 
                match cf with
                | Zen.Types.Extracted.CostFunc(_, cf) -> cf, mf
         
        let inputTxSkeleton = 
            //run_fsx inputFilePath
            Consensus.ZFStar.convertInput Consensus.TxSkeleton.empty
       
        
        ()
                     
    | _ ->
        showUsage()
    
    0 // return an integer exit code
