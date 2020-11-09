
open Consensus
open Types
open Zen.Types.Data
open Zen.Data
open Infrastructure

module Cost = Zen.Cost.Realized

// Contract Arguments
let contractId = ContractId (Version0, Hash.fromString "34f5f7f90d135aa4e596961c04a4fa41bfa4b81e1362f720d7b02a7a551cc4fa" |> Result.get)

let contractFn, costFn = System.Reflection.Assembly.LoadFrom "output/NamedToken.dll"
                         |> Contract.getFunctions
                         |> Result.get

let command = ""
let sender = Anonymous
let wallet : list<PointedOutput> = []
let context = {blockNumber=1ul;timestamp=0UL}

// Data with return address
let returnAddress = PK Hash.zero |> ZFStar.fsToFstLock |> Lock

let data =
    Zen.Dictionary.add "returnAddress"B returnAddress Zen.Dictionary.empty
    |> Cost.__force
    |> Dict
    |> Collection
    |> Some

// Transaction with one input
let tx : TxSkeleton.T =
    {
        pInputs=
            [
                TxSkeleton.PointedOutput ({txHash=Hash.zero;index=0ul},{lock=PK Hash.zero;spend={asset=Asset.Zen;amount=1UL}})
            ];
        outputs=
            [

            ]
    }
    
// Empty state
let state : data option = None

match contractFn tx context contractId command sender data wallet state with
| Ok (tx, message, stateUpdate) ->
    printfn "main fn result:\n tx: %A\n message: %A\n state update: %A" tx message stateUpdate
| Error error ->
    printfn "main fn error: %A" error
