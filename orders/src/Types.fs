module Types

open System
open System.Text.Json.Serialization
open System.Threading.Tasks
open Confluent.Kafka

type CreateOrderCommand =
    { [<JsonPropertyName("orderId")>]
      OrderId: Guid

      [<JsonPropertyName("customerId")>]
      CustomerId: int

      [<JsonPropertyName("productId")>]
      ProductId: int

      [<JsonPropertyName("amount")>]
      Amount: decimal }

type CreateOrderRequest =
    { CustomerId: int
      ProductId: int
      Amount: decimal }

type Order =
    { OrderId: Guid
      State: string
      CustomerId: int
      ProductId: int
      Amount: decimal }

type GetOrder = Guid -> Task<Order option>

type CreateOrder = Order -> Task<unit>

type UpdateOrder = Order -> Task<unit>

type StartSaga = CreateOrderCommand -> Task<unit>

type Environment =
    { Consumer: IConsumer<string, string>
      StartSaga: StartSaga
      GetOrder: GetOrder
      CreateOrder: CreateOrder
      UpdateOrder: UpdateOrder }

type Event =
    { [<JsonPropertyName("sagaId")>]
      SagaId: Guid
      [<JsonPropertyName("orderId")>]
      OrderId: Guid
      [<JsonPropertyName("type")>]
      Type: string
      [<JsonPropertyName("success")>]
      Success: bool }
