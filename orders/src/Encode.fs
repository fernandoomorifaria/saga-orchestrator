module Encode

open Thoth.Json.Net
open Types

let createOrder (command: CreateOrderCommand) =
    Encode.object
        [ "orderId", Encode.guid command.OrderId
          "customerId", Encode.int command.CustomerId
          "productId", Encode.int command.ProductId
          "amount", Encode.decimal command.Amount ]

let status (status: OrderStatus) =
    match status with
    | Placed -> "Placed"
    | Cancelled -> "Cancelled"
