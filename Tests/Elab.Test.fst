module Elab.Test

open Zen.Cost

val x : int
let x = 3

val foo : int -> cost bool 3
let foo x =
    match x with
    | 3 -> ret true
    | _ -> ret false
