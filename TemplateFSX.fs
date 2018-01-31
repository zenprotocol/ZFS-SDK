module TemplateFSX

open System.IO
module String = FSharpx.String

let generate (sourceFilePath : string) : array<string> =
    let contractModuleName = sourceFilePath.Substring(7, sourceFilePath.Length-11)
    [| sprintf "#r \"%s\"" sourceFilePath
       ""
       sprintf "open %s" contractModuleName
       ""
       "let mainFunction = mainFunction"
       ""
       sprintf "printfn \"Hello World, I am a test for the %s contract!\"" contractModuleName
    |]
