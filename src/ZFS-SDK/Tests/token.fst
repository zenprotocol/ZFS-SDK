open Zen.Types
open Zen.Vector
open Zen.Util
open Zen.Base
open Zen.Cost

module ET = Zen.ErrorT

val cf: txSkeleton -> string -> cost nat 1
let cf _ _ = ret 1000

val main: txSkeleton -> hash -> string -> cost (result txSkeleton) 1000
let main txSkeleton contractHash command =
 let zenToken:asset = hashFromBase64 "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=" in
 let pk = hashFromBase64 "A+vjRaDBgl5uS8mDug2t3PTABRHM01A8IpVlJRMBFjWT" in

 do tokens <-- getAvailableTokens zenToken txSkeleton;

 lockToContract {asset=zenToken;amount=tokens} contractHash txSkeleton
 >>= mint tokens contractHash
 >>= lockToPubKey {asset=contractHash;amount=tokens} pk
 >>= ET.retT
