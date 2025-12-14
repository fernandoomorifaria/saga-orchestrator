module Decode

open Thoth.Json.Net
open Types

let order: Decoder<Order> =
    Decode.object (fun get ->
        { OrderId = get.Required.Field "orderId" Decode.guid
          CustomerId = get.Required.Field "customerId" Decode.int
          ProductId = get.Required.Field "productId" Decode.int
          Amount = get.Required.Field "amount" Decode.decimal })

let replyType: Decoder<ReplyType> =
    Decode.string
    |> Decode.andThen (fun str ->
        match str with
        | "inventory.reserved" -> Decode.succeed InventoryReserved
        | "inventory.out_of_stock" -> Decode.succeed OutOfStock
        | "inventory.released" -> Decode.succeed InventoryReleased
        | "payments.processed" -> Decode.succeed PaymentProcessed
        | "payments.insufficient_funds" -> Decode.succeed InsufficientFunds
        | _ -> Decode.fail $"Unknown ReplyType: {str}")

let reply: Decoder<Reply> =
    Decode.object (fun get ->
        { SagaId = get.Required.Field "sagaId" Decode.guid
          OrderId = get.Required.Field "orderId" Decode.guid
          Type = get.Required.Field "type" replyType })

let state (str: string) =
    match str with
    | "Pending" -> Pending
    | "Completed" -> Completed
    | "Failed" -> Failed
