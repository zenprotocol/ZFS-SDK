
open System.IO
open Utils
open ContractTemplate
open Argu



module Create =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the file to be created"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult Filepath |> fun filepath -> 
            if Path.GetExtension(filepath) = ".fst" then
                let code = sprintf codeT (filepath.Substring (0, filepath.Length - 4))
                System.IO.File.WriteAllText (filepath, code)
                log "Created %s" filepath
                Ok filepath
            else
                Error "File extension must be .fst"



module Elaborate =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
        | [<Unique>] [<AltCommandLine("-z")>] Z3rlimit of z3rlimit : int
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the file to be elaborated"
                | Z3rlimit _ -> "Specify the rlimit to Z3"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult    Filepath |> fun filepath ->
        args.TryGetResult Z3rlimit |> fun z3rlimit ->
            ZFS.elab_file filepath
            >>= ZFS.verify z3rlimit



module Verify =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
        | [<Unique>] [<AltCommandLine("-z")>] Z3rlimit of z3rlimit : int
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the file to be verified"
                | Z3rlimit _ -> "Specify the rlimit to Z3"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult    Filepath |> fun filepath ->
        args.TryGetResult Z3rlimit |> fun z3rlimit ->
            ZFS.verify z3rlimit filepath



module Extract =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
        | [<Unique>] [<AltCommandLine("-z")>] Z3rlimit of z3rlimit : int
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the file to be extracted"
                | Z3rlimit _ -> "Specify the rlimit to Z3"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult    Filepath |> fun filepath ->
        args.TryGetResult Z3rlimit |> fun z3rlimit ->
            ZFS.elab_file filepath
            >>= ZFS.extract z3rlimit



module Compile =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
        | [<Unique>] [<AltCommandLine("-z")>] Z3rlimit of z3rlimit : int
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the file to be compiled"
                | Z3rlimit _ -> "Specify the rlimit to Z3"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult    Filepath |> fun filepath ->
        args.TryGetResult Z3rlimit |> fun z3rlimit ->
            ZFS.elab_file filepath
            >>= ZFS.extract z3rlimit
            >>= ZFS.compile



module Pack =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the file to be packed"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult Filepath |> fun filepath ->
            ZFS.pack filepath



module GenerateFSX =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                |  Filepath _ -> "Specify the name of the .fsx file to be generated"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult Filepath |> fun filepath ->
            Fsx.generate filepath



module RunFSX =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the .fsx file to be executed"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult Filepath |> fun filepath ->
            Fsx.run filepath



module ContractId =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the contract"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult Filepath |> fun filepath ->
            ContractId.showContractId filepath



module ACost =
    
    type Cmd =
        | [<MainCommand; ExactlyOnce>] Filepath of filepath : string
        | [<Unique>] [<AltCommandLine("-n")>] NumOfBlocks of numOfBlocks : uint32
    with
        interface IArgParserTemplate with
            member arg.Usage =
                match arg with
                | Filepath _ -> "Specify the name of the contract"
                | NumOfBlocks _ -> "Specify the number of blocks you want the contract to be active for"
    
    let handle (args : ParseResults<Cmd>) =
        args.GetResult Filepath       |> fun filepath    ->
        args.TryGetResult NumOfBlocks |> fun numOfBlocks ->
            Cost.showTotalCost Consensus.Chain.mainParameters filepath (numOfBlocks |> Option.defaultValue 1ul)



type CliArguments =
    | [<CliPrefix(CliPrefix.None)>]
        Create of ParseResults<Create.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-e")>]
        Elaborate of ParseResults<Elaborate.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-v")>]
        Verify of ParseResults<Verify.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-x")>]
        Extract of ParseResults<Extract.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-c")>]
        Compile of ParseResults<Compile.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-p")>]
        Pack of ParseResults<Pack.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-g")>] [<CustomCommandLine("generate-fsx")>]
        GenerateFSX of ParseResults<GenerateFSX.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-r")>] [<CustomCommandLine("run-fsx")>]
        RunFSX of ParseResults<RunFSX.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-d")>] [<CustomCommandLine("cid")>]
        ContractId of ParseResults<ContractId.Cmd>
    | [<CliPrefix(CliPrefix.None)>] [<AltCommandLine("-a")>]
        ACost of ParseResults<ACost.Cmd>
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Create      _ -> "Create a new template contract"
            | Elaborate   _ -> "Elaborate the source file and verify"
            | Verify      _ -> "Verify the source file"
            | Extract     _ -> "Extract the source file"
            | Compile     _ -> "Compile from source file"
            | Pack        _ -> "Compile from source file"
            | GenerateFSX _ -> "Generate a .fsx file to test the contract with"
            | ContractId  _ -> "Compute contract id"
            | RunFSX      _ -> "Run the given .fsx file, automatically loading Zen DLLs"
            | ACost       _ -> "Calculate activation cost" 


let parser = ArgumentParser.Create<CliArguments>(programName = "zebra.exe")

let usage = parser.PrintUsage()

let usage' = """
USAGE: zebra [--z3rlimit <int>] [< option >] <source file>

PARAMS:
    <source file>      The ZF* source file to use

OPTIONS:
    --create                Create a new template contract
    --elaborate, -e         Elaborate the source file and verify
    --verify, -v            Verify the source file
    --extract, -x           Extract the source file
    --compile, -c           Compile from source file
    --pack, -p              Pack from source file
    --generate-fsx, -g      Generate a .fsx file to test the contract with
    --run-fsx, -r           Run the given .fsx file, automatically loading Zen dlls.
    --cid                   Compute contract id
    --z3rlimit <int>        Specify the rlimit to z3.
"""


let handleCommand =
    function
    | Create args ->
        Create.handle args
    | Elaborate args ->
        Elaborate.handle args
    | Verify args ->
        Verify.handle args
    | Extract args ->
        Extract.handle args
    | Compile args ->
        Compile.handle args
    | Pack args ->
        Pack.handle args
    | GenerateFSX args ->
        GenerateFSX.handle args
    | ContractId args ->
        ContractId.handle args
    | RunFSX args ->
        RunFSX.handle args
    | ACost args ->
        ACost.handle args

[<EntryPoint>]
let main argv =
    try
        parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
        |> fun cmds ->
            match cmds.GetAllResults() with
            | [] ->
                printfn "%s" usage
            | cmd :: _ ->
                handleCommand cmd
                |> ignore
    with e ->
        printfn "%s" e.Message
    0
