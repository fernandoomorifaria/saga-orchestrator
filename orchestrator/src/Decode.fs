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

let reserveInventory: Decoder<ReserveInventoryCommand> =
    Decode.object (fun get ->
        { SagaId = get.Required.Field "sagaId" Decode.guid
          OrderId = get.Required.Field "orderId" Decode.guid
          Type = get.Required.Field "type" Decode.string
          ProductId = get.Required.Field "productId" Decode.int })

let releaseInventory: Decoder<ReleaseInventoryCommand> =
    Decode.object (fun get ->
        { SagaId = get.Required.Field "sagaId" Decode.guid
          OrderId = get.Required.Field "orderId" Decode.guid
          Type = get.Required.Field "type" Decode.string
          ProductId = get.Required.Field "productId" Decode.int })

let processPayment: Decoder<ProcessPaymentCommand> =
    Decode.object (fun get ->
        { SagaId = get.Required.Field "sagaId" Decode.guid
          OrderId = get.Required.Field "orderId" Decode.guid
          Type = get.Required.Field "type" Decode.string
          CustomerId = get.Required.Field "customerId" Decode.int
          Amount = get.Required.Field "amount" Decode.decimal })

let command: Decoder<Command> =
    Decode.field "type" Decode.string
    |> Decode.andThen (fun commandType ->
        match commandType with
        | "ReserveInventory" -> reserveInventory |> Decode.map ReserveInventory
        | "ReleaseInventory" -> releaseInventory |> Decode.map ReleaseInventory
        | "ProcessPayment" -> processPayment |> Decode.map ProcessPayment
        | _ -> Decode.fail $"Unknown Command type: {commandType}")

let state: Decoder<State> =
    Decode.string
    |> Decode.andThen (fun str ->
        match str with
        | "pending" -> Decode.succeed Pending
        | "completed" -> Decode.succeed Completed
        | "failed" -> Decode.succeed Failed
        | _ -> Decode.fail $"Unknown State: {str}")
