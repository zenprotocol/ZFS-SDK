module Compile.Test

open Zen.Types
open Zen.ErrorT
open Zen.Cost

val main : transactionSkeleton -> cost (result transactionSkeleton) 1
let main transactionSkeleton = failw "Not Implemented"

val cf : transactionSkeleton -> cost nat 1
let cf _ = ret 1