module Types

open System
open System.Threading.Tasks
open Confluent.Kafka

type CreateOrderCommand =
    { OrderId: Guid
      CustomerId: int
      ProductId: int
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

type OrderStatus =
    | Placed
    | Cancelled

type OrderEvent = { OrderId: Guid; Status: OrderStatus }
