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

namespace Reward.Machines.Slack.Command
#nowarn "62"
#light "off"

open Aether.Operators
open FParsec
open FParsec.Pipes
open Freya.Core
open Freya.Machines.Http
open Freya.Types.Http
open Freya.Optics
open Reward.Dto
open Reward.Freya
open Reward.Machines.Slack
open Reward.Optics.Operators
open Reward.Yolo

[<AutoOpen>]
module __reward__ = begin
  type Action = FastHelp
              | Help

  let parseCommand =
    let phelp = %% "help" -%> Help in
    let p = phelp in
    fun text ->
      match run p text with
      | Success (res, _, _) -> res
      | _                   -> FastHelp

  let parseBody =
    Http.Request.body_
    >-> Slack.Dto.Command.stream_
    >?> Slack.Domain.Command.dto_
    |> Freya.Optic.get
    |> Freya.memo

  let representHelp command =
    let userId_ =
      (Aether.Prism.ofEpimorphism Slack.Dto.Help.json_)
      >?> Slack.Domain.Help.dto_
      >?> Slack.Domain.Help.userId_
    in
    let invoker = command^.Slack.Domain.Command.invoker_ in
    invoker^=userId_

  let representCommand = freya
  { let! command' = parseBody in
    let command = command'.Value in
    let represent = representHelp command in
    return { Description={ Charset=Some Charset.Utf8
                         ; Encodings=None
                         ; MediaType=Some MediaType.Json
                         ; Languages=None
                         }
           ; Data=Utf8.bytes (represent "")
           }
  }

  let machine = freyaMachine
  { methods POST
  ; acceptableMediaTypes MediaType.Form
  ; allowed verifySlackRequest
  ; badRequest (parseBody |> Freya.map Option.isNone)
  ; handleOk representCommand
  }
end

type RewardCommandMachine = Reward
  with
    static member Pipeline(_) = HttpMachine.Pipeline(machine)
  end
