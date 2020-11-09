#light "off"
module NamedToken
open Prims
open FStar.Pervasives

let main = (fun ( txSkeleton  :  Zen.Types.Realized.txSkeleton ) ( uu____101  :  'Auu____51 ) ( contractId  :  Zen.Types.Extracted.contractId ) ( command  :  'Auu____52 ) ( sender  :  'Auu____53 ) ( messageBody  :  Zen.Types.Data.data FStar.Pervasives.Native.option ) ( wallet  :  'Auu____54 ) ( state  :  'Auu____55 ) -> (Zen.Cost.Realized.inc 405L 54L (

let dict = (Zen.Data.op_Greater_Bang_Equals 4L messageBody Zen.Data.tryDict)
in (Zen.Cost.Extracted.letBang 70L 335L (Zen.Data.op_Greater_Question_Equals 68L 2L (Zen.Data.op_Greater_Question_Equals 4L 64L dict (Zen.Dictionary.tryFind "returnAddress"B)) Zen.Data.tryLock) (fun ( returnAddress  :  Zen.Types.Extracted.lock FStar.Pervasives.Native.option ) -> (Zen.Cost.Extracted.letBang 70L 265L (Zen.Data.op_Greater_Question_Equals 68L 2L (Zen.Data.op_Greater_Question_Equals 4L 64L dict (Zen.Dictionary.tryFind "amount"B)) Zen.Data.tryU64) (fun ( amount  :  FStar.UInt64.t FStar.Pervasives.Native.option ) -> (Zen.Cost.Extracted.letBang 70L 195L (Zen.Data.op_Greater_Question_Equals 68L 2L (Zen.Data.op_Greater_Question_Equals 4L 64L dict (Zen.Dictionary.tryFind "name"B)) Zen.Data.tryString) (fun ( name  :  Prims.string FStar.Pervasives.Native.option ) -> (match (((returnAddress), (amount), (name))) with
| (FStar.Pervasives.Native.Some (returnAddress1), FStar.Pervasives.Native.Some (amount1), FStar.Pervasives.Native.Some (name1)) -> begin
(match (((FStar.String.length name1) <= 32L)) with
| true -> begin
(Zen.Cost.Extracted.letBang 64L 131L (Zen.Asset.fromSubtypeString contractId name1) (fun ( token  :  Zen.Types.Extracted.asset ) -> (Zen.Cost.Extracted.letBang 128L 3L (Zen.Cost.Realized.bind 64L 64L (Zen.TxSkeleton.lockToAddress token amount1 returnAddress1 txSkeleton) (Zen.TxSkeleton.mint amount1 token)) (fun ( txSkeleton1  :  Zen.Types.Realized.txSkeleton ) -> (Zen.ContractResult.ofTxSkel txSkeleton1)))))
end
| uu____246 -> begin
(Zen.ResultT.autoFailw 195L "name is too long"B)
end)
end
| uu____247 -> begin
(Zen.ResultT.autoFailw 195L "parameters are missing"B)
end))))))))))


let cf = (fun ( uu____343  :  'Auu____302 ) ( uu____344  :  'Auu____303 ) ( uu____345  :  'Auu____304 ) ( uu____346  :  'Auu____305 ) ( uu____347  :  'Auu____306 ) ( wallet  :  'Auu____307 ) ( uu____349  :  'Auu____308 ) -> (Zen.Cost.Realized.inc 0L 31L (Zen.Base.op_Bar_Greater (Zen.Base.op_Bar_Greater 459L Zen.Base.cast) Zen.Cost.Realized.ret)))


let mainFunction : Zen.Types.Main.mainFunction = Zen.Types.Main.MainFunc (Zen.Types.Main.CostFunc (31L, cf), main)




