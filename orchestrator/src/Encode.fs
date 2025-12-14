module Encode

open Thoth.Json.Net
open Types

let order (order: Order) =
    Encode.object
        [ "orderId", Encode.guid order.OrderId
          "customerId", Encode.int order.CustomerId
          "productId", Encode.int order.ProductId
          "amount", Encode.decimal order.Amount ]

let replyType (replyType: ReplyType) =
    match replyType with
    | InventoryReserved -> Encode.string "inventory.reserved"
    | OutOfStock -> Encode.string "inventory.out_of_stock"
    | InventoryReleased -> Encode.string "inventory.released"
    | PaymentProcessed -> Encode.string "payments.processed"
    | InsufficientFunds -> Encode.string "payments.insufficient_funds"

let reply (reply: Reply) =
    Encode.object
        [ "sagaId", Encode.guid reply.SagaId
          "orderId", Encode.guid reply.OrderId
          "type", replyType reply.Type ]

let reserveInventory (command: ReserveInventoryCommand) =
    Encode.object
        [ "sagaId", Encode.guid command.SagaId
          "orderId", Encode.guid command.OrderId
          "type", Encode.string command.Type
          "productId", Encode.int command.ProductId ]

let releaseInventory (command: ReleaseInventoryCommand) =
    Encode.object
        [ "sagaId", Encode.guid command.SagaId
          "orderId", Encode.guid command.OrderId
          "type", Encode.string command.Type
          "productId", Encode.int command.ProductId ]

let processPayment (command: ProcessPaymentCommand) =
    Encode.object
        [ "sagaId", Encode.guid command.SagaId
          "orderId", Encode.guid command.OrderId
          "type", Encode.string command.Type
          "customerId", Encode.int command.CustomerId
          "amount", Encode.decimal command.Amount ]

let command (command: Command) =
    match command with
    | ReserveInventory command -> reserveInventory command
    | ReleaseInventory command -> releaseInventory command
    | ProcessPayment command -> processPayment command

let state (state: State) =
    match state with
    | Pending -> Encode.string "pending"
    | Completed -> Encode.string "completed"
    | Failed -> Encode.string "failed"
