#r "mscorlib.dll"
#r "System.Core.dll"
#r "System.dll"
#r "System.Numerics.dll"
#I "../bin/Debug/"
#r "../bin/Debug/Consensus.dll"
#r "../bin/Debug/Zulib.dll"
#I "bin"
#r "Compile.Test.dll"

open Compile.Test
open Zen.Types.Extracted

type Nonce = nonce

let contractMain = mainFunction
