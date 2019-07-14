(******************************************************************************)
(* Copyright (C) 2019 Contributors as noted in the AUTHORS.md file            *)
(*                                                                            *)
(* This file is part of Reward.vision back office.                            *)
(*                                                                            *)
(* This program is free software: you can redistribute it and/or modify       *)
(* it under the terms of the GNU Affero General Public License as published   *)
(* by the Free Software Foundation, either version 3 of the License, or       *)
(* (at your option) any later version.                                        *)
(*                                                                            *)
(* This program is distributed in the hope that it will be useful,            *)
(* but WITHOUT ANY WARRANTY; without even the implied warranty of             *)
(* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the              *)
(* GNU Affero General Public License for more details.                        *)
(*                                                                            *)
(* You should have received a copy of the GNU Affero General Public License   *)
(* along with this program.  If not, see <https://www.gnu.org/licenses/>.     *)
(******************************************************************************)

module Reward.Yolo
#nowarn "62"
#nowarn "9"
#light "off"

open System

module Bytes = begin
  open System.Runtime.InteropServices
  open Microsoft.FSharp.NativeInterop

  let lookup32 = Array.zeroCreate<uint32> 256
  let lookup32P = GCHandle.Alloc(lookup32, GCHandleType.Pinned)
                          .AddrOfPinnedObject()
                  |> NativePtr.ofNativeInt<uint32>

  do for i = 0 to 255 do
    let s = i.ToString("x2") in
    if BitConverter.IsLittleEndian
    then lookup32.[i] <- (uint32 s.[0]) + ((uint32 s.[1]) <<< 16)
    else lookup32.[i] <- (uint32 s.[1]) + ((uint32 s.[0]) <<< 16)
  done

  let toStringHex : byte array -> _ = function
  | [||] | null -> String.Empty
  | bytes       -> let res = String('\x00', bytes.Length * 2) in
                   use bytesP = fixed bytes in
                   use resP = fixed res in
                   for i = 0 to bytes.Length-1 do
                     let resP32 = resP |> NativePtr.toNativeInt
                                       |> NativePtr.ofNativeInt<uint32> in
                     let value = NativePtr.get bytesP i
                                 |> int
                                 |> NativePtr.get lookup32P in
                     NativePtr.set resP32 i value
                   done;
                   res
end

module Utf8 = begin
  let bytes (s:string) =
    Text.Encoding.UTF8.GetBytes(s)
end

module Option = begin
  let apply f v = 
    let inline ( >>= ) v f = Option.bind f v in
    f >>= fun f -> v >>= fun v -> Some (f v)

  let lift2 f a b =
    apply (apply (Some f) a) b
end

module List = begin
  let inline cons hd tl = hd::tl

  let traverseOptionA f list =
    let inline ( <*> ) f v = Option.apply f v in
    let folder hd tl = Some cons <*> f hd <*> tl in
    List.foldBack folder list (Some [])

  let sequenceOptionA v = traverseOptionA id v

  let traverseFreyaA f list =
    let inline ( <*> ) f v = Freya.Core.Freya.apply v f in
    let inline result v = Freya.Core.Freya.init v in
    let folder hd tl = result cons <*> f hd <*> tl in
    List.foldBack folder list (result [])

  let sequenceFreyaA v = traverseFreyaA id v

  let toTuple = function
  | [a;b] -> Some (a, b)
  | _     -> None
end
