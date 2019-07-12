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

[<AutoOpen>]
module Reward.Machines.Slack.__main__
#nowarn "62"
#light "off"

open System
open Aether
open Aether.Operators
open Freya.Core
open Freya.Optics.Http
open Reward.Config
open Reward.Optics
open Reward.Yolo

let memoryBody =
  Freya.Optic.map
    Request.body_
    <| fun ks ->
         let copy = new IO.MemoryStream() in
         ks.CopyTo(copy);
         copy.Position <- 0L;
         upcast copy

let hmac =
  let bytes = Utf8.bytes Config.slack.signingSecret in
  new Security.Cryptography.HMACSHA256(bytes)

let verifySlackRequest = freya
{ let inline ( <|> ) v f = Option.filter f v in
  let inline ( <!> ) v f = Option.map f v in
  let now = DateTimeOffset.UtcNow.ToUnixTimeSeconds() in
  do! memoryBody in
  let! shash = Freya.Optic.get (Request.header_ "X-Slack-Signature") in
  let! headers =
    [ Request.header_ "X-Slack-Request-Timestamp" >-> Option.value_
    ; Request.body_ >-> String.stream_
    ] |> List.traverseFreyaA Freya.Optic.get
      |> Freya.map List.sequenceOptionA
      |> Freya.map (Option.bind List.toTuple)
  in
  do! Freya.Optic.set (Request.body_ >-> Stream.pos_) 0L in
  return headers
  <|> ( fun data ->
          data^.(fst_ >-> Int64.string_)
          <|> (( - ) now >> ( > ) 10L)
          |> Option.isSome
      )
  <!> ( fun (ts, body) ->
          let base' = String.Join(':', "v0", ts, body) in
          let hash = hmac.ComputeHash(Utf8.bytes base') in
          "v0=" + Bytes.toStringHex hash
      )
  |> Option.lift2 ( = ) shash <|> id
  |> Option.isSome
}
