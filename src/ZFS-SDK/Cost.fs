module Cost

open Infrastructure.Result

module Contract = Consensus.Contract
module ZFStar = Infrastructure.ZFStar

[<Literal>]
let rlimit =
    2723280u

type ChainParameters =
    Consensus.Chain.ChainParameters

type ContractCost =
    {
        blockSacrifice : uint64
        totalSacrifice : uint64
        fee            : uint64
        
    }

let Version0 =
    Consensus.Types.Version0

let result =
    new ResultBuilder<string>()

module private Util =
    
    let private getModuleName : Consensus.Hash.Hash -> string =
        id
        >> Consensus.Hash.bytes
        >> FsBech32.Base16.encode
        >> (+) "Z"
    
    let private computeHash (version : uint32) (code : string) =
            
            let versionBytes =
                Infrastructure.BigEndianBitConverter.uint32ToBytes version
            
            let codeBytes =
                System.Text.Encoding.UTF8.GetBytes code
            
            Consensus.Hash.computeMultiple [versionBytes; codeBytes]
    
    let recordHints (code:string) : Result<string, string> =
        code
        |> computeHash Version0
        |> getModuleName
        |> ZFStar.recordHints rlimit code

let computeCosts
    ( chain       : ChainParameters )
    ( code        : string          )
    ( numOfBlocks : uint32          )
    : Result<ContractCost, string> =
    result {
        let contractId =
            Contract.makeContractId Version0 code
        
        let! hints =
            Measure.measure
                (sprintf "recording hints for contract %A" contractId)
                (lazy Util.recordHints code)
        
        let! queries =
            ZFStar.totalQueries hints
        
        let codeLength =
            String.length code |> uint64
        
        return
            {
                blockSacrifice =
                    chain.sacrificePerByteBlock * codeLength
                
                totalSacrifice =
                    chain.sacrificePerByteBlock * codeLength * (uint64 numOfBlocks)
                
                fee =
                    queries * rlimit / 100ul |> uint64
            }
    }

let totalCost (cost : ContractCost) : uint64 =
    cost.fee + cost.totalSacrifice

let showTotalCost
    ( chain       : ChainParameters )
    ( filepath    : string          )
    ( numOfBlocks : uint32          )
    : Result<string, string> =
    try
        let code = System.IO.File.ReadAllText filepath
        computeCosts chain code numOfBlocks
        <@> fun cost ->
            printfn "number of blocks = %d" numOfBlocks
            printfn "base sacrifice = %d" cost.blockSacrifice
            printfn "total sacrifice = %d" cost.totalSacrifice 
            printfn "fee = %d" cost.fee
            printfn "total = %d" (totalCost cost)
            (totalCost cost).ToString()
    with _ as e ->
        Error (sprintf "%A" e)