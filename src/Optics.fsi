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

module String = begin
  /// Isomorphism between streams and strings.
  ///
  /// **Consumes the entire stream for conversion.**
  /// As `Freya.Optics.Http.Request.body_` is a Kestrel stream by default,
  /// you can only consume it once. Call `Reward.Machines.memoryBody` in
  /// your Freya workflow to convert it to a regular `System.IO.MemoryStream`.
  val stream_ : Aether.Isomorphism< System.IO.Stream
                                  , string
                                  >
end

module Stream = begin
  /// Lens to the Position property of a stream.
  ///
  /// #### Setter
  /// May raise `System.NotSupportedException` on streams who do not
  /// support seeking.
  val pos_ : Aether.Lens< System.IO.Stream
                        , int64
                        >
end

module Int64 = begin
  /// Epimorphism between strings and int64.
  val string_ : Aether.Epimorphism< string
                                  , int64
                                  >
end
