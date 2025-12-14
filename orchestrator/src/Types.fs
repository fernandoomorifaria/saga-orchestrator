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

type CommandType =
    | ReserveInventory
    | ReleaseInventory
    | ProcessPayment

type ReserveInventoryCommand =
    { SagaId: Guid
      OrderId: Guid
      Type: CommandType
      ProductId: int }

type ReleaseInventoryCommand =
    { SagaId: Guid
      OrderId: Guid
      Type: CommandType
      ProductId: int }

type ProcessPaymentCommand =
    { SagaId: Guid
      OrderId: Guid
      Type: CommandType
      CustomerId: int
      Amount: decimal }

type Command =
    | ReserveInventoryCommand of ReserveInventoryCommand
    | ReleaseInventoryCommand of ReleaseInventoryCommand
    | ProcessPaymentCommand of ProcessPaymentCommand

type OrderStatus =
    | Placed
    | Cancelled

type OrderEvent = { OrderId: Guid; Status: OrderStatus }
