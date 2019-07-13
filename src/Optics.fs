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

module Reward.Optics
#nowarn "62"
#light "off"

open System
open Aether

module String = begin
  let stream_ : Isomorphism<IO.Stream, _> =
    ( ( fun s ->
          let reader = new IO.StreamReader(s) in
          reader.ReadToEnd()
      )
    , ( fun s ->
          let bytes = Text.Encoding.UTF8.GetBytes(s) in
          upcast new IO.MemoryStream(bytes)
      )
    )
end

module Stream = begin
  let pos_ : Lens<IO.Stream, _> =
    (fun s -> s.Position), (fun p s -> s.Position <- p; s)
end

module Int64 = begin
  let string_ : Epimorphism<string, _> =
    ( ( fun s ->
          let mutable result = Unchecked.defaultof<int64> in
          if Int64.TryParse(s, &result) then Some result else None
      )
    , string
    )
end
