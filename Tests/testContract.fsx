#I "bin"
#r "Compile.Test.dll"
#I "../bin/Debug/"
#r "Consensus.dll"
#r "Zulib.dll"
#r "System.Reflection.Metadata.dll"
#r "FSharp.Compatibility.OCaml.dll"
#r "mscorlib.dll"
#r "System.Core.dll"
#r "System.dll"
#r "System.Numerics.dll"

open Compile.Test
open Zen.Types.Extracted

type Nonce = nonce

let contractMain = mainFunction
