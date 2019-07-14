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
#light "off"

module Bytes = begin
  /// Treats a byte array as a big-endian integer, and returns its
  /// hexadecimal representation in lowercase, with no prefix nor suffix.
  ///
  /// #### Example
  /// ```fsharp
  /// toStringHex [|10uy..16uy|];; // => "0a0b0c0d0e0f10"
  /// toStringHex null;; // => ""
  /// ```
  val toStringHex : byte array
                 -> string
end

module Utf8 = begin
  val bytes : string
           -> byte array
end

module Option = begin
  val apply : ('α -> 'β) option
           -> ('α option -> 'β option)

  val lift2 : ('α -> 'β -> 'γ)
           -> ('α option -> 'β option -> 'γ option)
end

module List = begin
  val traverseOptionA : ('α -> 'β option)
                     -> ('α list -> 'β list option)

  val traverseFreyaA : ('α -> 'β Freya.Core.Freya)
                    -> ('α list -> 'β list Freya.Core.Freya)

  val sequenceOptionA : 'α option list
                     -> 'α list option

  val sequenceFreyaA : 'α Freya.Core.Freya list
                    -> 'α list Freya.Core.Freya

  val toTuple : 'α list
             -> ('α * 'α) option
end
