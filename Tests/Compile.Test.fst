module Compile.Test

open Zen.Types
open Zen.ErrorT
open Zen.Cost

val main : inputMsg -> cost (result transactionSkeleton) 1
let main inputMsg = failw "Not Implemented"

val cf : inputMsg -> cost nat 1
let cf _ = ret 1