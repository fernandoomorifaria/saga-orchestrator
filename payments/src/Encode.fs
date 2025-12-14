module Encode

open Thoth.Json.Net
open Types

let replyType (replyType: ReplyType) =
    match replyType with
    | PaymentProcessed -> Encode.string "payments.processed"
    | InsufficientFunds -> Encode.string "payments.insufficient_funds"

let reply (reply: Reply) =
    Encode.object
        [ "sagaId", Encode.guid reply.SagaId
          "orderId", Encode.guid reply.OrderId
          "type", replyType reply.Type ]
