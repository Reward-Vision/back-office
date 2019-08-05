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

module Dto = begin
  module Challenge = begin
    type T = FSharp.Data.JsonProvider<"src/Dto/slack_challenge.json">.Root

    val stream_ : Aether.Epimorphism<System.IO.Stream, T>
  end

  module UserId = begin
    type T

    val nvc_ : Aether.Prism<System.Collections.Specialized.NameValueCollection, T>
    val mrkdwn_ : Aether.Epimorphism<string, T>
  end

  type Help;;
  type Help with
    static member json_ : Aether.Epimorphism<string, Help>
  end

  module Command = begin
    type T

    val stream_ : Aether.Epimorphism<System.IO.Stream, T>
  end
end

module Domain = begin
  module Challenge = begin
    type T

    val dto_ : Aether.Epimorphism<Dto.Challenge.T, T>

    val value_ : Aether.Lens<T, string>
  end

  type UserId;;
  type UserId with
    static member dto_ : Aether.Isomorphism<Dto.UserId.T, UserId>
    static member id_ : Aether.Lens<UserId, string>
    static member name_ : Aether.Lens<UserId, string>
  end

  type Help;;
  type Help with
    static member dto_ : Aether.Isomorphism<Dto.Help, Help>
    static member userId_ : Aether.Lens<Help, UserId>
  end

  module Command = begin
    type T

    val dto_ : Aether.Epimorphism<Dto.Command.T, T>

    val text_ : Aether.Lens<T, string>
    val invoker_ : Aether.Lens<T, UserId>
    val triggerId_ : Aether.Lens<T, string>
  end
end
