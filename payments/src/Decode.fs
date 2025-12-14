module Decode

open Thoth.Json.Net
open Types

let processPayment: Decoder<ProcessPaymentCommand> =
    Decode.object (fun get ->
        { SagaId = get.Required.Field "sagaId" Decode.guid
          OrderId = get.Required.Field "orderId" Decode.guid
          Type = get.Required.Field "type" Decode.string
          CustomerId = get.Required.Field "customerId" Decode.int
          Amount = get.Required.Field "amount" Decode.decimal })
