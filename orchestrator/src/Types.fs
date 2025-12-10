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

(* TODO: Use ADT *)
type SagaMessage =
    { [<JsonPropertyName("sagaId")>]
      SagaId: Guid

      [<JsonPropertyName("state")>]
      State: string

      [<JsonPropertyName("type")>]
      Type: string

      [<JsonPropertyName("order")>]
      Order: Order }

type Saga =
    { SagaId: Guid
      State: string
      Order: Order }

type SagaEntity =
    { Id: int
      SagaId: Guid
      State: string
      Order: string
      CreatedAt: DateTime
      LastUpdatedAt: DateTime }
