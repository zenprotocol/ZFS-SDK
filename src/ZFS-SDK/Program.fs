module Program

open Argu



[<Literal>]
let DEFAULT_Z3RLIMIT =
    2723280u 

let (>>=) x f =
    Result.bind f x

let handle_log_types (x : Option<'a>) : string =
    x
    |> Option.map (fun _ -> "--log_types")
    |> Option.defaultValue ""

let handle_log_queries (x : Option<'a>) : string =
    x
    |> Option.map (fun _ -> "--log_queries")
    |> Option.defaultValue ""



module Create =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "Name of the generated contract"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        let code =
            ContractTemplate.code filename
        
        System.IO.File.WriteAllText(filename, code)

        Utils.log "Created %s" filename

        Ok filename



module Elaborate =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        | [<Unique>]
            Z3rlimit of uint32
        
        | [<Unique>]
            Log_Types
        
        | [<Unique>]
            Log_Queries
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "File name of the contract to elaborate"
                    | Z3rlimit _ ->
                        "Z3 rlimit"
                    | Log_Types ->
                        "Log types"
                    | Log_Queries ->
                        "Log Z3 queries"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        let z3rlimit =
            args.TryGetResult(Z3rlimit)
        
        let logtypes =
            handle_log_types <| args.TryGetResult(Log_Types)
        
        let logqueries =
            handle_log_queries <| args.TryGetResult(Log_Queries)
        
        ZFS.elab_file filename
        >>= ZFS.verify z3rlimit [logtypes; logqueries]



module Verify =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        | [<Unique>]
            Z3rlimit of uint32
        
        | [<Unique>]
            Log_Types
        
        | [<Unique>]
            Log_Queries
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "File name of the contract to verify"
                    | Z3rlimit _ ->
                        "Z3 rlimit"
                    | Log_Types ->
                        "Log types"
                    | Log_Queries ->
                        "Log Z3 queries"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        let z3rlimit =
            args.TryGetResult(Z3rlimit)
        
        let logtypes =
            handle_log_types <| args.TryGetResult(Log_Types)
        
        let logqueries =
            handle_log_queries <| args.TryGetResult(Log_Queries)
        
        ZFS.verify z3rlimit [logtypes; logqueries] filename



module Extract =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        | [<Unique>]
            Z3rlimit of uint32
        
        | [<Unique>]
            Log_Types
        
        | [<Unique>]
            Log_Queries
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "File name of the contract to extract"
                    | Z3rlimit _ ->
                        "Z3 rlimit"
                    | Log_Types ->
                        "Log types"
                    | Log_Queries ->
                        "Log Z3 queries"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        let z3rlimit =
            args.TryGetResult(Z3rlimit)
        
        let logtypes =
            handle_log_types <| args.TryGetResult(Log_Types)
        
        let logqueries =
            handle_log_queries <| args.TryGetResult(Log_Queries)
        
        ZFS.elab_file filename
        >>= ZFS.extract z3rlimit [logtypes; logqueries]



module Compile =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        | [<Unique>]
            Z3rlimit of uint32
        
        | [<Unique>]
            Log_Types
        
        | [<Unique>]
            Log_Queries
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "File name of the contract to compile"
                    | Z3rlimit _ ->
                        "Z3 rlimit"
                    | Log_Types ->
                        "Log types"
                    | Log_Queries ->
                        "Log Z3 queries"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        let z3rlimit =
            args.TryGetResult(Z3rlimit)
        
        let logtypes =
            handle_log_types <| args.TryGetResult(Log_Types)
        
        let logqueries =
            handle_log_queries <| args.TryGetResult(Log_Queries)
        
        ZFS.elab_file filename
        >>= ZFS.extract z3rlimit [logtypes; logqueries]
        >>= ZFS.compile



module Pack =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "File name of the contract to pack"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename = args.GetResult(Filename)
        
        ZFS.pack filename



module Generate_Fsx =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "File name of the source contract to generate FSX script from"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        Fsx.generate filename



module Run_Fsx =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "File name of FSX script"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        Fsx.run filename



module Contract_Id =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename _ ->
                        "File name of contract"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        ZFS.contractId filename



module ACost =
    
    type Arg =
        
        | [<MainCommand>]
            Filename of string
        
        |
            NumOfBlocks of uint32
        
        |
            Z3rlimit of uint32
        
        with
            
            interface IArgParserTemplate with
                
                member s.Usage =
                    match s with
                    | Filename    _ ->
                        "File name of contract"
                    | NumOfBlocks _ ->
                        "Number of blocks"
                    | Z3rlimit    _ ->
                        "Z3 rlimit"
    
    
    let handle (args : ParseResults<Arg>) : Result<string, string> =
        
        let filename =
            args.GetResult(Filename)
        
        let numOfBlocks =
            args.GetResult(NumOfBlocks)
        
        let z3rlimit =
            args.TryGetResult(Z3rlimit)
            |> Option.defaultValue DEFAULT_Z3RLIMIT
        
        ZFS.activationCost z3rlimit filename numOfBlocks



type Command =
    
    | [<CliPrefix(CliPrefix.None)>]
        Create
        of ParseResults<Create.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("e")>]
        Elaborate
        of ParseResults<Elaborate.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("v")>]
        Verify
        of ParseResults<Verify.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("x")>]
        Extract
        of ParseResults<Extract.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("c")>]
        Compile
        of ParseResults<Compile.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("p")>]
        Pack
        of ParseResults<Pack.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("g")>]
        Generate_Fsx
        of ParseResults<Generate_Fsx.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("r")>]
        Run_Fsx
        of ParseResults<Run_Fsx.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("cid")>]
        Contract_Id
        of ParseResults<Contract_Id.Arg>
    
    | [<CliPrefix(CliPrefix.None); AltCommandLine("ac")>]
        ACost
        of ParseResults<ACost.Arg>
    
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
                
                | Contract_Id _ ->
                    "Compute contract ID."
                
                | ACost _ ->
                    "Compute activation cost."




let parser =
    ArgumentParser.Create<Command>(programName = "zebra")


let usage =
    parser.PrintUsage()


[<EntryPoint>]
let main argv =
    
    if Array.isEmpty argv then printfn "%s" usage
    
    try
        let results =
            parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
        
        match results.GetAllResults() with
        
        | [Create args] ->
            Create.handle args
        
        | [Elaborate args] ->
            Elaborate.handle args
        
        | [Verify args] ->
            Verify.handle args
        
        | [Extract args] ->
            Extract.handle args
        
        | [Compile args] ->
            Compile.handle args
        
        | [Pack args] ->
            Pack.handle args
        
        | [Generate_Fsx args] ->
            Generate_Fsx.handle args
        
        | [Run_Fsx args] ->
            Run_Fsx.handle args
        
        | [Contract_Id args] ->
            Contract_Id.handle args
        
        | [ACost args] ->
            ACost.handle args
        
        | _ ->
            Error "Invalid argument"
        
        |> ignore
        0
    with e ->
        eprintf "%s" e.Message
        1
