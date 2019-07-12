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

namespace Reward.Dto.Slack
#nowarn "62"
#light "off"

open System
open Aether
open FSharp.Data

module Dto = begin
  module Challenge = begin
    type Impl = JsonProvider<"src/Dto/slack_challenge.json">
    type T = Impl.Root

    let stream_ : Epimorphism<IO.Stream, T> =
    ( (fun s -> try Some (Impl.Load(s)) with _ -> None)
    , ( fun json ->
          let writer = new IO.StreamWriter(new IO.MemoryStream()) in
          json.JsonValue.WriteTo(writer, JsonSaveOptions.DisableFormatting);
          writer.BaseStream
      )
    )
  end
end

module Domain = begin
  module Challenge = begin
    type T = T of string

    [<Literal>] let TYPE = "url_verification";;

    let dto_ : Epimorphism<Dto.Challenge.T, _> =
    ( ( fun dto ->
          if dto.Type = TYPE
          then Some (T dto.Challenge)
          else None
      )
    , ( fun (T v) ->
          Dto.Challenge.T(token="<#ERROR#>", challenge=v, ``type``=TYPE)
      )
    )

    let value_ : Lens<_, _> = (fun (T v) -> v), (fun v _ -> T v)
  end
end
