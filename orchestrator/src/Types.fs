module Types

open System
open System.Text.Json.Serialization

type Order =
    { [<JsonPropertyName("orderId")>]
      OrderId: Guid

      [<JsonPropertyName("customerId")>]
      CustomerId: int

      [<JsonPropertyName("productId")>]
      ProductId: int

      [<JsonPropertyName("amount")>]
      Amount: decimal }

type Saga =
    { SagaId: Guid
      State: string
      CurrentStep: int
      Order: Order }

type SagaStep =
    { Name: string
      CommandTopic: string
      ReplyTopic: string
      CompensationCommand: string }

type SagaEntity =
    { Id: int
      SagaId: Guid
      State: string
      CurrentStep: int
      Order: string
      CreatedAt: DateTime
      LastUpdatedAt: DateTime option }

type Event =
    { [<JsonPropertyName("sagaId")>]
      SagaId: Guid
      [<JsonPropertyName("orderId")>]
      OrderId: Guid
      [<JsonPropertyName("type")>]
      Type: string
      [<JsonPropertyName("success")>]
      Success: bool }

type ReserveInventoryCommand =
    { [<JsonPropertyName("sagaId")>]
      SagaId: Guid
      [<JsonPropertyName("orderId")>]
      OrderId: Guid
      [<JsonPropertyName("type")>]
      Type: string
      [<JsonPropertyName("productId")>]
      ProductId: int }

type ProcessPaymentCommand =
    { [<JsonPropertyName("sagaId")>]
      SagaId: Guid
      [<JsonPropertyName("orderId")>]
      OrderId: Guid
      [<JsonPropertyName("type")>]
      Type: string
      [<JsonPropertyName("customerId")>]
      CustomerId: int
      [<JsonPropertyName("amount")>]
      Amount: decimal }

type Command =
    | ReserveInventory of ReserveInventoryCommand
    | ProcessPayment of ProcessPaymentCommand
