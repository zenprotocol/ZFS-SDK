module ZFS_SDK.Program2

open System.IO
open Utils
open ContractTemplate

let usage = """
USAGE: zebra [--z3rlimit <int>] [< option >] <source file>

PARAMS:
    <source file>      The ZF* source file to use

OPTIONS:
    --create                Create a new template contract
    --elaborate, -e         Elaborate the source file and verify
    --verify, -v            Verify the source file
    --extract, -x           Extract the source file
    --compile, -c           Compile from source file
    --pack, -p              Pack the contract to be activated on zen blockchain
    --generate-fsx, -g      Generate a .fsx file to test the contract with
    --run-fsx, -r           Run the given .fsx file, automatically loading Zen dlls.
    --cid                   Compute contract id
    --z3rlimit <int>        Specify the rlimit to z3.
"""

type ContractProcess =
    {
        z3rlimit : int
        filePath : string
    }

type CostParams =
    {
        numOfBlocks : uint32
        filePath    : string
    }

type Parameter =
    | Create      of filePath : string
    | Elaborate   of ContractProcess
    | Verify      of ContractProcess
    | Extract     of ContractProcess
    | Compile     of ContractProcess
    | Pack        of ContractProcess
    | GenerateFSX of filePath : string
    | ContractId  of filePath : string
    | RunFSX      of filePath : string
    | ACost       of CostParams 

let rec main = function
    | Create filePath when Path.GetExtension(filePath) = ".fst" ->
        
        let code = sprintf codeT (filePath.Substring (0, filePath.Length - 4))

        System.IO.File.WriteAllText (filePath, code)

        log "Created %s" filePath

        Ok filePath
    
    | Elaborate proc ->
        ZFS.elab_file proc.filePath
        >>= ZFS.verify
    
    | Verify proc ->
        ZFS.verify proc.filePath
    
    | Extract proc ->
        ZFS.elab_file proc.filePath
        >>= ZFS.extract
    
    | Compile proc ->
        ZFS.elab_file proc.filePath
        >>= ZFS.extract
        >>= ZFS.compile
    
    | Pack proc ->
        ZFS.pack proc.filePath
    
    | GenerateFSX filePath ->
        Fsx.generate filePath
    
    | ContractId filePath ->
        ContractId.showContractId filePath
    
    | RunFSX filePath ->
        Fsx.run filePath

    | ACost costParams ->
        Cost.showTotalCost Consensus.Chain.mainParameters costParams.filePath costParams.numOfBlocks