module Types

open System
open System.Data
open Confluent.Kafka

type ReplyType =
    | PaymentProcessed
    | InsufficientFunds

type Reply =
    { SagaId: Guid
      OrderId: Guid
      Type: ReplyType }

type ProcessPaymentCommand =
    { SagaId: Guid
      OrderId: Guid
      Type: string
      CustomerId: int
      Amount: decimal }

type WalletEntity =
    { Id: int
      CustomerId: int
      Balance: decimal }

type Environment =
    { Producer: IProducer<string, string>
      Consumer: IConsumer<string, string>
      Connection: IDbConnection }
