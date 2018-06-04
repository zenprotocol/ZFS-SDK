open System.IO
open Utils
open ZFS

let usage = """
USAGE: zebra [< option >] <source file>

PARAMS:
    <source file>      The ZF* source file to use

OPTIONS:
    --create                Create a new template contract
    --elaborate, -e         Elaborate the source File and verify
    --verify, -v            Verify the source file
    --extract, -x           Extract the source file
    --compile, -c           Compile from source file
    --pack, -p              Pack the contract to be activated on zen blockchain
    --generate-fsx, -g      Generate a .fsx file to test the contract with
    --run-fsx, -r           Run the given .fsx file, automatically loading Zen dlls.
"""


let showUsage() =
    printfn "%s" usage

let expected extension filePath =
    if Path.GetExtension(filePath) <> extension then
        log "Expected %s extension" extension
        false
    else if not <| File.Exists filePath then
        log "File not found: %s" filePath
        false
    else
        true

[<EntryPoint>]
let main = function
    | [| option; filePath |] ->
        match option with
        | "--create" when Path.GetExtension(filePath) = ".fst" ->

            let code = sprintf """module %s

open Zen.Types
open Zen.Base
open Zen.Cost
open Zen.Asset
open Zen.Data
open Zen.Dictionary

module RT = Zen.ResultT
module OT = Zen.OptionT
module Tx = Zen.TxSkeleton
module CR = Zen.ContractResult.NoMessage

let buy txSkeleton contractId returnAddress =
  let! tokens = Tx.getAvailableTokens zenAsset txSkeleton in

  let! contractAsset = getDefault contractId in

  let! txSkeleton =
    Tx.lockToContract zenAsset tokens contractId txSkeleton
    >>= Tx.mint tokens contractAsset
    >>= Tx.lockToAddress contractAsset tokens returnAddress in

  CR.ret txSkeleton

let redeem txSkeleton contractId returnAddress wallet =
  let! contractAsset = getDefault contractId in
  let! tokens = Tx.getAvailableTokens contractAsset txSkeleton in

  let! txSkeleton =
    Tx.destroy tokens contractAsset txSkeleton
    >>= Tx.lockToAddress zenAsset tokens returnAddress
    >>= Tx.fromWallet zenAsset tokens contractId wallet in

  CR.ofOption "contract doesn't have enough zens to pay you" txSkeleton

let main txSkeleton context contractId command sender data wallet =
  let! returnAddress = data >!= tryCollection >?= tryDict >?= tryFind "returnAddress" >?= tryLock in

  match returnAddress with
  | Some returnAddress ->
      if command = "redeem" then
        redeem txSkeleton contractId returnAddress wallet
      else if command = "" || command = "buy" then
        buy txSkeleton contractId returnAddress
        |> autoInc
      else
        RT.autoFailw "unsupported command"
  | None ->
      RT.autoFailw "returnAddress is required"

let cf _ _ _ _ _ wallet =
    (2 + 2 + 64 + 2 + (64 + (64 + (64 + 64 + (Zen.Wallet.size wallet * 128 + 192) + 0)) + 25) + 33)
    |> cast nat
    |> ret"""           (filePath.Substring (0, filePath.Length - 4))

            System.IO.File.WriteAllText (filePath, code)

            log "Created %s" filePath

            Ok filePath

        | "-e" | "--elaborate" when expected ".fst" filePath ->
            ZFS.elab_file filePath
            >>= ZFS.verify

        | "-v" | "--verify" when expected ".fst" filePath ->
            ZFS.verify filePath

        | "-x" | "--extract" when expected ".fst" filePath ->
            ZFS.elab_file filePath
            >>= ZFS.extract

        | "-c" | "--compile" when expected ".fst" filePath ->
            ZFS.elab_file filePath
            >>= ZFS.extract
            >>= ZFS.compile

        | "-g" | "--generate-fsx" when expected ".fst" filePath ->
            Fsx.generate filePath

        | "-r" | "--run-fsx" when expected ".fsx" filePath ->
            Fsx.run filePath

        | "-p" | "--pack" when expected ".fst" filePath ->
            ZFS.pack filePath

        | _ ->
            Error ""
        |> function
        | Error error ->
            if error <> "" then log "Error: %A" error
            1
        | Ok _ ->
            0
    | _ ->
        showUsage()
        0