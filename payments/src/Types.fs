module Types

open System
open System.Data
open System.Threading.Tasks
open Confluent.Kafka
open Microsoft.Extensions.Logging

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
    { Publish: string -> string -> string -> Task<unit>
      Consumer: IConsumer<string, string>
      Connection: IDbConnection
      Logger: ILogger }
