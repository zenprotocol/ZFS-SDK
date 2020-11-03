module Tests

open NUnit.Framework
open Argu

open FSharp.Data
open Program


[<Literal>]
let CONTRACT_FILENAME =
    "NamedToken.fst"

[<Literal>]
let OUTPUT_DIR =
    "output"


let TestsDir =
    NUnit.Framework.TestContext.CurrentContext.TestDirectory
    |> System.IO.Path.GetDirectoryName
    |> System.IO.Path.GetDirectoryName

let TestsContractsDir =
    System.IO.Path.Combine [| TestsDir ; "TestsContracts" |]

let initialize () =
    System.IO.Directory.SetCurrentDirectory TestsContractsDir

let compareTextFiles filename1 filename2 =

    let file1 =
        System.IO.File.ReadAllText filename1

    let file2 =
        System.IO.File.ReadAllText filename2

    if file1 <> file2 then
        for i in 0 .. min file1.Length file2.Length - 1 do
            if file1.Chars i <> file2.Chars i then
                failwithf "character %d in %s is <%c> while in %s it is <%c>"
                    i filename1 (file1.Chars i) filename2 (file2.Chars i)
        if file1.Length > file2.Length then
            failwithf "%s is longer than %s" filename1 filename2
        elif file1.Length < file2.Length then
            failwithf "%s is longer than %s" filename2 filename1
        else
            failwithf "this should be impossible, something went horribly wrong"



[<TestFixture>]
module Create =

    [<Literal>]
    let ORIGINAL_CREATED_FILENAME =
        "OriginalCreate.fst"

    [<Literal>]
    let NEW_CREATED_FILENAME =
        "Create.fst"

    [<SetUp>]
    let SetUp () = ()

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

        if Program.main [| "create" ; NEW_CREATED_FILENAME |] <> 0 then
            failwith "Creation failed"

    [<OneTimeTearDown>]
    let OneTimeTearDown () =
        System.IO.File.Delete NEW_CREATED_FILENAME

    [<Test>]
    let ``Created file should be created`` () =
        if not <| System.IO.File.Exists NEW_CREATED_FILENAME then
            failwithf "Can't find %s - file wasn't created" NEW_CREATED_FILENAME

    [<Test>]
    let ``Created file should be as expected`` () =

        compareTextFiles ORIGINAL_CREATED_FILENAME NEW_CREATED_FILENAME



[<TestFixture>]
module Elaborate =

    [<Literal>]
    let ORIGINAL_ELABORATED_CONTRACT_FILENAME =
        "NamedToken_Elaborated.fst"

    let newElaboratedFilename =
        System.IO.Path.Combine [| OUTPUT_DIR ; CONTRACT_FILENAME |]

    [<SetUp>]
    let SetUp () = ()

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

        if Program.main [| "elaborate" ; CONTRACT_FILENAME |] <> 0 then
            failwith "Elaboration failed"

    [<OneTimeTearDown>]
    let OneTimeTearDown () =

        System.IO.Directory.Delete ( OUTPUT_DIR , true )

    [<Test>]
    let ``Elaborated file should be created`` () =
        if not <| System.IO.File.Exists newElaboratedFilename then
            failwithf "Can't find %s - elaborated file wasn't created" newElaboratedFilename

    [<Test>]
    let ``Elaborated file should be as expected`` () =

        compareTextFiles ORIGINAL_ELABORATED_CONTRACT_FILENAME newElaboratedFilename



[<TestFixture>]
module Verify =

    [<SetUp>]
    let SetUp () = ()

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

    [<OneTimeTearDown>]
    let OneTimeTearDown () = ()



[<TestFixture>]
module Extract =

    [<Literal>]
    let ORIGINAL_EXTRACTED_CONTRACT_FILENAME =
        "NamedToken_Extracted.fs"

    let newExtractedFilename =
        System.IO.Path.Combine [| OUTPUT_DIR ; System.IO.Path.ChangeExtension ( CONTRACT_FILENAME , "fs" ) |]

    [<SetUp>]
    let SetUp () = ()

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

        if Program.main [| "extract" ; CONTRACT_FILENAME |] <> 0 then
            failwith "Extraction failed"

    [<OneTimeTearDown>]
    let OneTimeTearDown () =
        System.IO.Directory.Delete ( OUTPUT_DIR , true )

    [<Test>]
    let ``Extracted file should be created`` () =
        if not <| System.IO.File.Exists newExtractedFilename then
            failwithf "Can't find %s - extracted file wasn't created" newExtractedFilename

    [<Test>]
    let ``Extracted file should be as expected`` () =

        compareTextFiles ORIGINAL_EXTRACTED_CONTRACT_FILENAME newExtractedFilename



[<TestFixture>]
module Compile =

    let compiledFilename =
        System.IO.Path.Combine [| OUTPUT_DIR ; System.IO.Path.ChangeExtension ( CONTRACT_FILENAME , "dll" ) |]

    [<SetUp>]
    let SetUp () = ()

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

        if Program.main [| "compile" ; CONTRACT_FILENAME |] <> 0 then
            failwith "Compilation failed"

    [<OneTimeTearDown>]
    let OneTimeTearDown () =
        System.IO.Directory.Delete ( OUTPUT_DIR , true )

    [<Test>]
    let ``Compiled file should be created`` () =

        if not <| System.IO.File.Exists compiledFilename then
            failwithf "Can't find %s - compiled file wasn't created" compiledFilename



[<TestFixture>]
module Pack =

    [<Literal>]
    let ORIGINAL_PACKED_FILENAME =
        "NamedToken_Packed.fst"

    [<Literal>]
    let NEW_PACKED_FILENAME =
        "Za999559e8567e7707e508890ca5a390fea808f2db35fc6a92cc639ad59ad28d9.fst"

    let mutable packedFilename = ""

    [<SetUp>]
    let SetUp () = ()

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

        let args =
            Program.Parser.Pack.Parse [| CONTRACT_FILENAME |]

        match Pack.handle Program.Parser.Pack args with
        | Error msg ->
            failwithf "Compilation failed - %s" msg
        | Ok filename ->
            packedFilename <- filename

    [<OneTimeTearDown>]
    let OneTimeTearDown () =
        System.IO.File.Delete NEW_PACKED_FILENAME

    [<Test>]
    let ``Packed file name should be as expected`` () =
        if packedFilename <> NEW_PACKED_FILENAME then
            failwithf "Packed filename should be %s but it is %s" NEW_PACKED_FILENAME packedFilename

    [<Test>]
    let ``Packed file should be as expected`` () =
        compareTextFiles ORIGINAL_PACKED_FILENAME packedFilename



[<TestFixture>]
module Generate_FSX =

    [<Literal>]
    let ORIGINAL_GENERATED_FILENAME =
        "NamedToken_Generate.fsx"

    [<Literal>]
    let NEW_GENERATED_FILENAME =
        "NamedToken.fsx"

    [<SetUp>]
    let SetUp () = ()

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

        if Program.main [| "generate-fsx" ; CONTRACT_FILENAME |] <> 0 then
            failwith "Generation failed"

    [<OneTimeTearDown>]
    let OneTimeTearDown () =
        System.IO.File.Delete NEW_GENERATED_FILENAME

    [<Test>]
    let ``Generated file should be created`` () =
        if not <| System.IO.File.Exists NEW_GENERATED_FILENAME then
            failwithf "Can't find %s - file wasn't generated" NEW_GENERATED_FILENAME

    [<Test>]
    let ``Generated file should be as expected`` () =

        compareTextFiles ORIGINAL_GENERATED_FILENAME NEW_GENERATED_FILENAME



[<TestFixture>]
module Run_FSX =

    [<SetUp>]
    let SetUp () =
        System.IO.Directory.SetCurrentDirectory TestsContractsDir

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () = ()

    [<OneTimeTearDown>]
    let OneTimeTearDown () = ()




[<TestFixture>]
module ContractId =

    [<Literal>]
    let expectedCid =
        "000000001d816175e1d8dca2f4e4fe33c963612ad28aee62dd55f31d2f685a113fe3d306"

    [<SetUp>]
    let SetUp () =
        System.IO.Directory.SetCurrentDirectory TestsContractsDir

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

    [<OneTimeTearDown>]
    let OneTimeTearDown () = ()

    [<Test>]
    let ``Contract ID should be as expected`` () =

        let args =
            Program.Parser.ContractId.Parse [| CONTRACT_FILENAME |]

        match ContractId.handle Program.Parser.ContractId args with
        | Error msg ->
            failwithf "Contract ID calculation failed - %s" msg
        | Ok cid ->
            if cid <> expectedCid then
                failwithf "Contract ID wasn't as expected - expected: %s ; got: %s" expectedCid cid



[<TestFixture>]
module ActivationCost =

    let expectedCost : Extra.ActivationCost.ActivationCost =
        {
            activationFee       = 925915UL
            baseSacrifice       = 1218UL
            activationSacrifice = 1218UL
            numOfBlocks         = 1u
            total               = 927133UL
        }

    [<SetUp>]
    let SetUp () =
        System.IO.Directory.SetCurrentDirectory TestsContractsDir

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

    [<OneTimeTearDown>]
    let OneTimeTearDown () = ()

    [<Test>]
    let ``Activation cost should be as expected`` () =

        let code =
            System.IO.File.ReadAllText Pack.ORIGINAL_PACKED_FILENAME

        match Extra.ActivationCost.compute Consensus.Chain.mainParameters Program.DEFAULT_Z3RLIMIT 1ul code with
        | Error msg ->
            failwithf "Activation cost calculation failed - %s" msg
        | Ok cost ->
            if cost <> expectedCost then
                failwithf "Cost wasn't as expected - expected: %A ; got: %A" expectedCost cost



[<TestFixture>]
module Info =

    [<Literal>]
    let EXPECTED_INFO_FILENAME =
        "NamedToken_Info.json"

    [<SetUp>]
    let SetUp () =
        System.IO.Directory.SetCurrentDirectory TestsContractsDir

    [<TearDown>]
    let TeardDown () = ()

    [<OneTimeSetUp>]
    let OneTimeSetUp () =

        initialize()

    [<OneTimeTearDown>]
    let OneTimeTearDown () = ()

    [<Test>]
    let ``Info should be as expected`` () =

        let expectedFile =
            System.IO.File.ReadAllText EXPECTED_INFO_FILENAME

        let expectedJson =
            JsonValue.Parse expectedFile

        let expectedProperties =
            expectedJson.Properties()

        let expectedInfo : Consensus.Types.ContractV0 =
            {
                code    = match (snd expectedProperties.[0]) with | JsonValue.String s -> s        | _ -> failwith "impossible"
                rlimit  = match (snd expectedProperties.[1]) with | JsonValue.String s -> uint32 s | _ -> failwith "impossible"
                hints   = match (snd expectedProperties.[2]) with | JsonValue.String s -> s        | _ -> failwith "impossible"
                queries = match (snd expectedProperties.[3]) with | JsonValue.String s -> uint32 s | _ -> failwith "impossible"
            }

        let code =
            System.IO.File.ReadAllText Pack.ORIGINAL_PACKED_FILENAME
        
        let info =
            match Extra.Info.compute Program.DEFAULT_Z3RLIMIT code with
            | Error msg ->
                failwithf "Couldn't compute contract info - %s" msg
            | Ok info ->
                info

        if info <> expectedInfo then
            failwithf "Cost wasn't as expected - expected: %A ; got: %A" expectedInfo info
