module NamedToken

open Zen.Types
open Zen.Base
open Zen.Cost
open Zen.Asset
open Zen.Data

module D = Zen.Dictionary
module Tx = Zen.TxSkeleton
module CR = Zen.ContractResult
module RT = Zen.ResultT
module String = FStar.String
module C = Zen.Cost

let main txSkeleton _ contractId command sender messageBody wallet state =
  (Zen.Cost.inc 54
      (let dict = messageBody >!= tryDict in
        let! returnAddress = dict >?= D.tryFind "returnAddress" >?= tryLock in
        let! amount = dict >?= D.tryFind "amount" >?= tryU64 in
        let! name = dict >?= D.tryFind "name" >?= tryString in
        match returnAddress, amount, name with
        | Some returnAddress, Some amount, Some name ->
          if String.length name <= 32
          then
            (let! token = Zen.Asset.fromSubtypeString contractId name in
              let! txSkeleton =
                Tx.lockToAddress token amount returnAddress txSkeleton >>= Tx.mint amount token in
              CR.ofTxSkel txSkeleton)
          else RT.autoFailw "name is too long"
        | _ -> RT.autoFailw "parameters are missing"))

let cf _ _ _ _ _ wallet _ =
  (Zen.Cost.inc 31
      ((4 + 64 + 2 + (4 + 64 + 2 + (4 + 64 + 2 + (64 + (64 + 64 + 3)))) + 54) |> cast nat |> C.ret))
val mainFunction: Zen.Types.mainFunction
let mainFunction = Zen.Types.MainFunc (Zen.Types.CostFunc cf) main