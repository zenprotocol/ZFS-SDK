module ContractId

module Contract = Consensus.Contract

let showContractId filepath =
    try
        let code = System.IO.File.ReadAllText filepath
        Contract.makeContractId 0ul code
        |> fun cid -> cid.ToString()
        |> fun s -> printfn "%s" s; sprintf "%s" s
        |> Ok
    with _ as e ->
        Error (sprintf "%A" e)