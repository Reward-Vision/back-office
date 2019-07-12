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

namespace Reward.Machines.Slack
#nowarn "62"
#light "off"

open Aether.Operators
open Freya.Core
open Freya.Machines.Http
open Freya.Types.Http
open Freya.Optics.Http
open Reward.Dto

[<AutoOpen>]
module private __challenge__ = begin
  let parseSlackChallenge =
    Request.body_
    >-> Slack.Dto.Challenge.stream_
    >?> Slack.Domain.Challenge.dto_
    |> Freya.Optic.get
    |> Freya.memo

  let representSlackChallenge = freya
  { let! challenge = parseSlackChallenge in
    let body = challenge.Value^.Slack.Domain.Challenge.value_ in
    return Represent.text body
  }

  let machine = freyaMachine
  { methods POST
  ; acceptableMediaTypes MediaType.Json
  ; allowed verifySlackRequest
  ; badRequest (parseSlackChallenge |> Freya.map Option.isNone)
  ; handleOk representSlackChallenge
  }
end

type ChallengeMachine = Challenge
  with
    static member Pipeline(_) = HttpMachine.Pipeline(machine)
  end
