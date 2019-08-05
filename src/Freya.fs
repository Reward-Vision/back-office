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

module Reward.Freya
#nowarn "62"
#light "off"

open Freya.Types.Http

type Freya.Types.Http.MediaType with
  static member Form = [ MediaType
                         ( Type "application"
                         , SubType "x-www-form-urlencoded"
                         , Parameters Map.empty
                         )
                       ]
end
