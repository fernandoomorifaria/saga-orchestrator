module Decode

open Thoth.Json.Net
open Types

let status: Decoder<OrderStatus> =
    Decode.string
    |> Decode.andThen (fun str ->
        match str with
        | "placed" -> Decode.succeed Placed
        | "cancelled" -> Decode.succeed Cancelled
        | _ -> Decode.fail $"Unknown order status: {str}")

let orderEvent: Decoder<OrderEvent> =
    Decode.object (fun get ->
        { OrderId = get.Required.Field "orderId" Decode.guid
          Status = get.Required.Field "status" status })
