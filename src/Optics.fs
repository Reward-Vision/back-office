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
open System.Web
open Aether

[<RequireQualifiedAccess>]
module Compose = begin
  type Isomorphism = Isomorphism with
    static member ( <.> ) (Isomorphism, (gb, sb):Isomorphism<'β, 'γ>) =
      fun ((ga, sa):Isomorphism<'α, 'β>) ->
        (ga >> gb, sb >> sa):Isomorphism<'α, 'γ>

    static member ( <.> ) (Isomorphism, (gb, sb):Epimorphism<'β, 'γ>) =
      fun ((ga, sa):Isomorphism<'α, 'β>) ->
        (ga >> gb, sb >> sa):Epimorphism<'α, 'γ>
  end

  type Epimorphism = Epimorphism with
    static member ( <?> ) (Epimorphism, (gb, sb):Isomorphism<'β, 'γ>) =
      fun ((ga, sa):Epimorphism<'α, 'β>) ->
        (ga >> Option.map gb, sb >> sa):Epimorphism<'α, 'γ>

    static member ( <?> ) (Epimorphism, (gb, sb):Epimorphism<'β, 'γ>) =
      fun ((ga, sa):Epimorphism<'α, 'β>) ->
        (ga >> Option.bind gb, sb >> sa):Epimorphism<'α, 'γ>
  end

  let inline isomorphism i m = Isomorphism <.> m <| i
  let inline epimorphism e m = Epimorphism <?> m <| e
end

module Operators = begin
  let inline ( <.> ) i m = Compose.isomorphism i m
  let inline ( <?> ) e m = Compose.epimorphism e m
end

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

module NameValueCollection = begin
  let httpQueryString_ : Epimorphism<_, _> =
    ( ( fun s ->
          try
            match HttpUtility.ParseQueryString(s) with
            | null -> None
            | seq  -> Some seq
          with
          | _ -> None
      )
    , fun seq -> seq.ToString()
    )
end
