module ContractId

open Infrastructure
open Consensus

let private computeHash version (code:string) =
    let versionBytes = BigEndianBitConverter.uint32ToBytes version
    let codeBytes = System.Text.Encoding.UTF8.GetBytes code

    Hash.computeMultiple [versionBytes; codeBytes]

let private makeContractId version code =
    Types.ContractId (version, computeHash version code)

let showContractId filepath =
    try
        let code = System.IO.File.ReadAllText filepath
        makeContractId 0ul code
        |> fun cid -> cid.ToString()
        |> fun s -> printfn "%s" s; sprintf "%s" s
        |> Ok
    with _ as e ->
        Error (sprintf "%A" e)