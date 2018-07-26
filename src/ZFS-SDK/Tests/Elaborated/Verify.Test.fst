
module Verify.Test

open Zen.Types
open Zen.ErrorT
open Zen.Cost

val main : inputMsg -> cost (result transactionSkeleton) 1
let main inputMsg = Zen.Cost.inc (failw "Not Implemented") 1

val cf : inputMsg -> cost nat 1
let cf _ = Zen.Cost.inc (ret 1) 1
val mainFunction : Zen.Types.mainFunction
let mainFunction = Zen.Types.MainFunc (Zen.Types.CostFunc cf) main