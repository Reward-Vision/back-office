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

module Reward.Config
#nowarn "62"
#light "off"

open FsConfig

type Slack = { signingSecret : string
             }

[<Convention("RWD")>]
type Config = { slack : Slack
              }

let Config =
  match EnvConfig.Get<Config>() with
  | Ok config                      -> config
  | Error (NotFound name)          -> failwithf "%s: required" name
  | Error (BadValue (name, value)) -> failwithf "%s: %s: bad value" name value
  | Error (NotSupported msg)       -> failwith msg
