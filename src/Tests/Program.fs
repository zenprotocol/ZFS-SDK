module Tests

open NUnit.Framework
open Argu

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
    let ``Created file should be as expected`` () =
        compareTextFiles ORIGINAL_CREATED_FILENAME NEW_CREATED_FILENAME 



[<TestFixture>]
module Elaborate =
    
    [<Literal>]
    let ORIGINAL_ELABORATED_CONTRACT_FILENAME =
        "NamedToken_Elaborated.fst"
    
    [<SetUp>]
    let SetUp () = ()
    
    [<TearDown>]
    let TeardDown () = ()
    
    [<OneTimeSetUp>]
    let OneTimeSetUp () =
        
        initialize()
    
    [<OneTimeTearDown>]
    let OneTimeTearDown () =
        System.IO.Directory.Delete ( OUTPUT_DIR , true )
    
    [<Test>]
    let ``Elaborated file should be as expected`` () =
        
        if Program.main [| "elaborate" ; CONTRACT_FILENAME |] <> 0 then
            failwith "Elaboration failed"
        
        let newElaboratedFilename =
            System.IO.Path.Combine [| OUTPUT_DIR ; CONTRACT_FILENAME |]
        
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
    
    [<SetUp>]
    let SetUp () = ()
    
    [<TearDown>]
    let TeardDown () = ()
    
    [<OneTimeSetUp>]
    let OneTimeSetUp () =
        
        initialize()
    
    [<OneTimeTearDown>]
    let OneTimeTearDown () =
        System.IO.Directory.Delete ( OUTPUT_DIR , true )
    
    [<Test>]
    let ``Extracted file should be as expected`` () =
        
        if Program.main [| "extract" ; CONTRACT_FILENAME |] <> 0 then
            failwith "Extraction failed"
        
        let newExtractedFilename =
            System.IO.Path.Combine [| OUTPUT_DIR ; System.IO.Path.ChangeExtension ( CONTRACT_FILENAME , "fs" ) |]
        
        compareTextFiles ORIGINAL_EXTRACTED_CONTRACT_FILENAME newExtractedFilename



[<TestFixture>]
module Compile =
    
    [<SetUp>]
    let SetUp () = ()
    
    [<TearDown>]
    let TeardDown () = ()
    
    [<OneTimeSetUp>]
    let OneTimeSetUp () =
        
        initialize()
    
    [<OneTimeTearDown>]
    let OneTimeTearDown () =
        System.IO.Directory.Delete ( OUTPUT_DIR , true )
    
    [<Test>]
    let ``Compiled file should be created`` () =
        
        if Program.main [| "compile" ; CONTRACT_FILENAME |] <> 0 then
            failwith "Compilation failed"
        
        let compiledFilename =
            System.IO.Path.Combine [| OUTPUT_DIR ; System.IO.Path.ChangeExtension ( CONTRACT_FILENAME , "dll" ) |]
        
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
module ActivationCost =
    
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
module Info =
    
    [<SetUp>]
    let SetUp () =
        System.IO.Directory.SetCurrentDirectory TestsContractsDir
    
    [<TearDown>]
    let TeardDown () = ()
    
    [<OneTimeSetUp>]
    let OneTimeSetUp () = ()
    
    [<OneTimeTearDown>]
    let OneTimeTearDown () = ()
