open System.IO
open Utils
open ZFS

let usage = """
USAGE: zebra [--z3rlimit <int>] [< option >] <source file>

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
    --z3rlimit <int>        Specify the rlimit to z3.
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
let rec main = function
    | [| option; filePath |] ->
        match option with
        | "--create" when Path.GetExtension(filePath) = ".fst" ->

            let code = sprintf """module %s

open Zen.Types
open Zen.Base
open Zen.Cost
open Zen.Asset
open Zen.Data

module D = Zen.Dictionary
module W = Zen.Wallet
module RT = Zen.ResultT
module Tx = Zen.TxSkeleton
module C = Zen.Cost
module CR = Zen.ContractResult

let buy txSkeleton contractId returnAddress =
  let! contractToken = Zen.Asset.getDefault contractId in
  let! amount = Tx.getAvailableTokens zenAsset txSkeleton in

  let! txSkeleton =
    Tx.lockToContract zenAsset amount contractId txSkeleton
    >>= Tx.mint amount contractToken
    >>= Tx.lockToAddress contractToken amount returnAddress in

  CR.ofTxSkel txSkeleton

let redeem txSkeleton contractId returnAddress wallet =
  let! contractToken = Zen.Asset.getDefault contractId in
  let! amount = Tx.getAvailableTokens contractToken txSkeleton in

  let! txSkeleton =
    Tx.destroy amount contractToken txSkeleton
    >>= Tx.lockToAddress zenAsset amount returnAddress
    >>= Tx.fromWallet zenAsset amount contractId wallet in

  CR.ofOptionTxSkel "contract doesn't have enough zens tokens" txSkeleton

let main txSkeleton _ contractId command sender messageBody wallet state =
  let! returnAddress =
    messageBody >!= tryDict
                >?= D.tryFind "returnAddress"
                >?= tryLock
  in
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

let cf _ _ _ _ _ wallet _ =
    4 + 64 + 2 + (64 + (64 + (64 + 64 + (Zen.Wallet.size wallet * 128 + 192) + 3)) + 25) + 31
    |> C.ret #nat"""           (filePath.Substring (0, filePath.Length - 4))

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
            if error <> "" then log "Error: %A" error else showUsage()
            1
        | Ok _ ->
            0
    | [| "--z3rlimit"; z3rlim; option; filePath |] ->
        ZFS.z3rlimit := System.Int32.Parse z3rlim
                        |> Some
        main [|option; filePath|]

    | _ ->
        showUsage()
        0
