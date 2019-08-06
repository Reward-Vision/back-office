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
open System.Linq
open System.Collections.Specialized
open Aether
open Aether.Operators
open Reward.Optics
open Reward.Optics.Operators
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

  module UserId = begin
    type T = T of string * string option

    let nvc_ : Prism<NameValueCollection, _> =
      ( ( fun nvc ->
            match nvc.["user_id"], nvc.["user_name"] with
            | null, _             -> None
            | uid, null | uid, "" -> Some (T (uid, None))
            | uid, uname          -> Some (T (uid, Some uname))
        )
      , ( fun (T (uid, uname)) nvc ->
            nvc.Add("user_id", uid);
            if uname.IsSome then nvc.Add("user_name", uname.Value);
            nvc
        )
      )

    let mrkdwn_ : Epimorphism<_, _> =
      ( (fun _ -> Some (T ("", Some "")))
      , ( fun (T (uid, uname)) ->
            let name = if uname.IsSome then "|" + uname.Value else "" in
            String.Concat("<@", uid, name, ">")
        )
      )
  end

  type FastHelp = FastHelp of UserId.T with
    static member json_ : Epimorphism<_, _> =
      let format:Printf.StringFormat<_> = """
{
  "response_type": "ephemeral",
  "text": "Hello %s! This doesn’t look like a valid command. Just type `/reward` to get started :smiley:",
  "attachments": [
    {
      "blocks": [
        {
          "type": "context",
          "elements": [
            {
              "type": "mrkdwn",
              "text": "For more info, type `/reward help`"
            }
          ]
        }
      ]
    }
  ]
}
      """ in
       ( (fst UserId.mrkdwn_ >> Option.map FastHelp)
      , ( fun (FastHelp userid) ->
            let uid = (snd UserId.mrkdwn_) userid in
            sprintf format uid
        )
      )

  end

  type Help = Help of UserId.T with
    static member json_ : Epimorphism<_, _> =
      let format:Printf.StringFormat<_> = """
{
  "response_type": "ephemeral",
  "text": "Hi %s, welcome to Reward!\n:point_right: Here’s the manual:",
  "attachments": [
    {
      "blocks": [
        {
          "type": "section",
          "text": {
            "type": "mrkdwn",
            "text": "*Commands*"
          }
        },
        {
          "type": "section",
          "text": {
            "type": "mrkdwn",
            "text": "• *Reward* a peer by typing `/reward`\n• *Check* your balance with `/reward balance`"
          }
        },
        {
          "type": "context",
          "elements": [
            {
              "type": "mrkdwn",
              "text": "*Tip:* You can also use `/reward @someone`"
            }
          ]
        }
      ]
    }
  ]
}
      """ in
      ( (fst UserId.mrkdwn_ >> Option.map Help)
      , ( fun (Help userid) ->
            let uid = (snd UserId.mrkdwn_) userid in
            sprintf format uid
        )
      )
  end

  module Command = begin
    type T = T of NameValueCollection

    let stream_ =
      String.stream_
      <.> NameValueCollection.httpQueryString_
      <?> (T, fun (T seq) -> seq)
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

  type UserId = { id : string
                ; name : string option
                } with
    static member dto_ : Isomorphism<Dto.UserId.T, _> =
      ( (fun (Dto.UserId.T (uid, uname)) -> {id=uid;name=uname})
      , (fun uid -> Dto.UserId.T (uid.id, uid.name))
      )

    static member id_ : Lens<_, _> =
      (fun u -> u.id), (fun id u -> {u with id=id})

    static member name_ : Lens<_, _> =
      ( (fun u -> Option.defaultValue "" u.name)
      , (fun name u -> {u with name=Some name})
      )
  end

  type FastHelp = { userid : UserId
                  } with
    static member dto_ : Isomorphism<Dto.FastHelp, _> =
      ( (fun (Dto.FastHelp userid) -> {userid=userid^.(id_ >-> UserId.dto_)})
      , (fun xx -> Dto.FastHelp ((snd UserId.dto_) xx.userid))
      )

    static member userId_ : Lens<_, _> =
      (fun xx -> xx.userid), (fun id xx -> {xx with userid=id})
  end

  type Help = { userid : UserId
                  } with
    static member dto_ : Isomorphism<Dto.Help, _> =
      ( (fun (Dto.Help userid) -> {userid=userid^.(id_ >-> UserId.dto_)})
      , (fun xx -> Dto.Help ((snd UserId.dto_) xx.userid))
      )

    static member userId_ : Lens<_, _> =
      (fun xx -> xx.userid), (fun id xx -> {xx with userid=id})
  end

  module Command = begin
    type T = { text : string
             ; invoker : UserId
             ; triggerId : string
             }

    let dto_ : Epimorphism<Dto.Command.T, _> =
      ( ( fun (Dto.Command.T dto) ->
            let invoker = dto^.Dto.UserId.nvc_ in
            let keys = ["text";"trigger_id"] in
            if invoker.IsSome && Seq.forall dto.AllKeys.Contains keys
            then Some { text=dto.["text"]
                      ; invoker=invoker.Value^.(Lens.ofIsomorphism UserId.dto_)
                      ; triggerId=dto.["trigger_id"]
                      }
            else None
        )
      , ( fun t ->
            let dto = NameValueCollection() in
            dto.Set("text", t.text);
            Dto.Command.T dto
        )
      )

    let text_ : Lens<_, _> =
      (fun xx -> xx.text), (fun text xx -> {xx with text=text})

    let invoker_ : Lens<_, _> =
      (fun xx -> xx.invoker), (fun invoker xx -> {xx with invoker=invoker})

    let triggerId_ : Lens<_, _> =
      (fun xx -> xx.triggerId), (fun tid xx -> {xx with triggerId=tid})
  end
end
