module Program

open Argu

[<Literal>]
let LOG_TYPES_FLAG =
    "--log_types"

[<Literal>]
let DEFAULT_Z3RLIMIT =
    2723280u

let (>>=) x f =
    Result.bind f x

let handle_log_types<'a> : Option<'a> -> string =
    Option.map (fun _ -> LOG_TYPES_FLAG)
    >> Option.defaultValue ""



module Strings =
    
    let FILENAME : Printf.StringFormat<string -> string> =
        "File name of the %s"
    
    [<Literal>]
    let Z3RLIMIT =
        "Z3 rlimit"
    
    [<Literal>]
    let LOG_TYPES =
        "Log types"
    
    [<Literal>]
    let NUM_OF_BLOCKS =
        "Number of blocks"



module Create =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "generated contract"
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            let code =
                ContractTemplate.code filename
            
            System.IO.File.WriteAllText(filename, code)
            Utils.log "Created %s" filename
            Ok filename



module Elaborate =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        | [<Unique ; AltCommandLine("-z")>]
            Z3rlimit of rlimit:uint32
        | [<Unique ; AltCommandLine("-t")>]
            Log_Types
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "contract to elaborate"
                    | Z3rlimit _ ->
                        Strings.Z3RLIMIT
                    | Log_Types ->
                        Strings.LOG_TYPES
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
        
            let filename =
                args.GetResult(Filename)
            
            let z3rlimit =
                args.TryGetResult(Z3rlimit)
            
            let logtypes =
                handle_log_types <| args.TryGetResult(Log_Types)
            
            ZFS.elab_file filename
            >>= ZFS.verify z3rlimit [logtypes]



module Verify =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        | [<Unique ; AltCommandLine("-z")>]
            Z3rlimit of rlimit:uint32
        | [<Unique ; AltCommandLine("-t")>]
            Log_Types
        with
            interface IArgParserTemplate with                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "contract to verify"
                    | Z3rlimit _ ->
                        Strings.Z3RLIMIT
                    | Log_Types ->
                        Strings.LOG_TYPES
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            let z3rlimit =
                args.TryGetResult(Z3rlimit)
            
            let logtypes =
                handle_log_types <| args.TryGetResult(Log_Types)
            
            ZFS.verify z3rlimit [logtypes] filename



module Extract =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        | [<Unique ; AltCommandLine("-z")>]
            Z3rlimit of rlimit:uint32
        | [<Unique ; AltCommandLine("-t")>]
            Log_Types
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "contract to extract"
                    | Z3rlimit _ ->
                        Strings.Z3RLIMIT
                    | Log_Types ->
                        Strings.LOG_TYPES
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            let z3rlimit =
                args.TryGetResult(Z3rlimit)
            
            let logtypes =
                handle_log_types <| args.TryGetResult(Log_Types)
            
            ZFS.elab_file filename
            >>= ZFS.extract z3rlimit [logtypes]



module Compile =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        | [<Unique ; AltCommandLine("-z")>]
            Z3rlimit of rlimit:uint32
        | [<Unique ; AltCommandLine("-t")>]
            Log_Types
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "contract to compile"
                    | Z3rlimit _ ->
                        Strings.Z3RLIMIT
                    | Log_Types ->
                        Strings.LOG_TYPES
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            let z3rlimit =
                args.TryGetResult(Z3rlimit)
            
            let logtypes =
                handle_log_types <| args.TryGetResult(Log_Types)
            
            ZFS.elab_file filename
            >>= ZFS.extract z3rlimit [logtypes]
            >>= ZFS.compile



module Pack =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "contract to pack"
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            ZFS.pack filename



module Generate_Fsx =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "source contract to generate FSX script from"
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            Fsx.generate filename



module Run_Fsx =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "FSX script"
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            Fsx.run filename



module ContractId =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename _ ->
                        sprintf Strings.FILENAME "contract"
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            ZFS.contractId filename



module ACost =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        | [<Unique ; AltCommandLine("-n")>]
            NumOfBlocks of uint32
        | [<Unique ; AltCommandLine("-z")>]
            Z3rlimit of rlimit:uint32
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename    _ ->
                        sprintf Strings.FILENAME "contract"
                    | NumOfBlocks _ ->
                        Strings.NUM_OF_BLOCKS
                    | Z3rlimit    _ ->
                        Strings.Z3RLIMIT
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            let numOfBlocks =
                args.GetResult(NumOfBlocks)
            
            let z3rlimit =
                args.TryGetResult(Z3rlimit)
                |> Option.defaultValue DEFAULT_Z3RLIMIT
            
            ZFS.activationCost z3rlimit filename numOfBlocks



module Info =
    
    type Arg =
        | [<MainCommand ; ExactlyOnce>]
            Filename of filename:string
        | [<Unique ; AltCommandLine("-z")>]
            Z3rlimit of rlimit:uint32
        with
            interface IArgParserTemplate with
                member s.Usage =
                    match s with
                    | Filename    _ ->
                        sprintf Strings.FILENAME "contract"
                    | Z3rlimit    _ ->
                        Strings.Z3RLIMIT
    
    
    let handle (parser : ArgumentParser<Arg>) (args : ParseResults<Arg>) : Result<string, string> =
        
        if List.isEmpty <| args.GetAllResults() then
            
            Ok <| parser.PrintUsage()
            
        else
            
            let filename =
                args.GetResult(Filename)
            
            let z3rlimit =
                args.TryGetResult(Z3rlimit)
                |> Option.defaultValue DEFAULT_Z3RLIMIT
        
            ZFS.getInfo z3rlimit filename


type Command =
    | [<CliPrefix(CliPrefix.None)>]
        Create of ParseResults<Create.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("e")>]
        Elaborate of ParseResults<Elaborate.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("v")>]
        Verify of ParseResults<Verify.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("x")>]
        Extract of ParseResults<Extract.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("c")>]
        Compile of ParseResults<Compile.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("p")>]
        Pack of ParseResults<Pack.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("g")>]
        Generate_Fsx of ParseResults<Generate_Fsx.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("r")>]
        Run_Fsx of ParseResults<Run_Fsx.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("cid")>]
        ContractId of ParseResults<ContractId.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("ac")>]
        ACost of ParseResults<ACost.Arg>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("i")>]
        Info of ParseResults<Info.Arg>
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Create _ ->
                    "Create a new template contract"
                | Elaborate _ ->
                    "Elaborate the source File and verify"
                | Verify _ ->
                    "Verify the source file"
                | Extract _ ->
                    "Extract the source file"
                | Compile _ ->
                    "Compile from source file"
                | Pack _ ->
                    "Pack the contract to be activated on zen blockchain"
                | Generate_Fsx _ ->
                    "Generate a .fsx file to test the contract with"
                | Run_Fsx _ ->
                    "Run the given .fsx file, automatically loading Zen dlls."
                | ContractId _ ->
                    "Compute contract ID."
                | ACost _ ->
                    "Compute activation cost."
                | Info _ ->
                    "Get contract information"



let parser =
    ArgumentParser.Create<Command>(programName = "zebra")


module Parser =
    let Create       = parser.GetSubCommandParser Create
    let Elaborate    = parser.GetSubCommandParser Elaborate
    let Verify       = parser.GetSubCommandParser Verify
    let Extract      = parser.GetSubCommandParser Extract
    let Compile      = parser.GetSubCommandParser Compile
    let Pack         = parser.GetSubCommandParser Pack
    let Generate_Fsx = parser.GetSubCommandParser Generate_Fsx
    let Run_Fsx      = parser.GetSubCommandParser Run_Fsx
    let ContractId   = parser.GetSubCommandParser ContractId
    let ACost        = parser.GetSubCommandParser ACost
    let Info         = parser.GetSubCommandParser Info


let usage =
    parser.PrintUsage()


let handleCommand =
    function
    | Create       args -> Create       .handle Parser.Create       args
    | Elaborate    args -> Elaborate    .handle Parser.Elaborate    args
    | Verify       args -> Verify       .handle Parser.Verify       args
    | Extract      args -> Extract      .handle Parser.Extract      args
    | Compile      args -> Compile      .handle Parser.Compile      args
    | Pack         args -> Pack         .handle Parser.Pack         args
    | Generate_Fsx args -> Generate_Fsx .handle Parser.Generate_Fsx args
    | Run_Fsx      args -> Run_Fsx      .handle Parser.Run_Fsx      args
    | ContractId   args -> ContractId   .handle Parser.ContractId   args
    | ACost        args -> ACost        .handle Parser.ACost        args
    | Info         args -> Info         .handle Parser.Info         args


let handleResults (results : ParseResults<Command>) : Result<string, string> =
    match results.GetAllResults() with
    | [] ->
        Ok usage
    | [cmd] ->
        handleCommand cmd
    | _ :: _ ->
        Error "Too many arguments"


let display output =
    printfn "%s" output
    0


let error msg =
    eprintfn "%s" msg
    1


[<EntryPoint>]
let main argv =
    try match handleResults <| parser.ParseCommandLine(inputs = argv, raiseOnUsage = true) with
        | Ok output ->
            display output
        | Error msg ->
            error msg
    with e ->
        error e.Message
