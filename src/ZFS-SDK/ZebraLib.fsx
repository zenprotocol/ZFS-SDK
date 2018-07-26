module Zebra

open Consensus

let load assembly =
    System.Reflection.Assembly.LoadFrom assembly
    |> Contract.getFunctions
    |> function | Ok fn -> fn | Error error -> failwith error

let (==>) (mainFn, costFn) fn =
    fn mainFn costFn

    mainFn,costFn




