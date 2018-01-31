module TemplateFSX

open System.IO
module String = FSharpx.String

let generate (sourceFilePath : string) : array<string> =
    let contractModuleName = 
        let noDllExtension = sourceFilePath.Remove(sourceFilePath.Length-4)
        noDllExtension.Remove(0, 7)
    [| sprintf "#r \"%s\"" sourceFilePath
       ""
       sprintf "open %s" contractModuleName
       ""
       "let mainFunction = mainFunction"
       ""
       sprintf "printfn \"Hello World, I am a test for the %s contract!\"" contractModuleName
    |]
