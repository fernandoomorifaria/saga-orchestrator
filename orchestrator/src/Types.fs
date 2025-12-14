module Types

open System

type Order =
    { OrderId: Guid
      CustomerId: int
      ProductId: int
      Amount: decimal }

type State =
    | Pending
    | Completed
    | Failed

type Saga =
    { SagaId: Guid
      State: State
      Order: Order }

type SagaEntity =
    { Id: int
      SagaId: Guid
      State: string
      Order: string
      CreatedAt: DateTime
      LastUpdatedAt: DateTime }

type ReplyType =
    | InventoryReserved
    | OutOfStock
    | InventoryReleased
    | PaymentProcessed
    | InsufficientFunds

type Reply =
    { SagaId: Guid
      OrderId: Guid
      Type: ReplyType }

type ReserveInventoryCommand =
    { SagaId: Guid
      OrderId: Guid
      Type: string
      ProductId: int }

type ReleaseInventoryCommand =
    { SagaId: Guid
      OrderId: Guid
      Type: string
      ProductId: int }

type ProcessPaymentCommand =
    { SagaId: Guid
      OrderId: Guid
      Type: string
      CustomerId: int
      Amount: decimal }

type Command =
    | ReserveInventory of ReserveInventoryCommand
    | ReleaseInventory of ReleaseInventoryCommand
    | ProcessPayment of ProcessPaymentCommand

type OrderStatus =
    | Placed
    | Cancelled

type OrderEvent = { OrderId: Guid; Status: OrderStatus }
