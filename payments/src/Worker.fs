namespace Payments

open System
open System.Threading
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Thoth.Json.Net
open Types

type Worker(environment: Environment) =
    inherit BackgroundService()

    let handleMessage (message: string) =
        task {
            let command = Decode.fromString Decode.processPayment message

            match command with
            | Ok command ->
                let orderId = command.OrderId.ToString()
                let customerId = command.CustomerId
                let amount = command.Amount

                let! wallet = Database.get environment.Connection customerId

                match wallet with
                (* TODO: Send some event about wallet not found *)
                | None -> ()
                | Some wallet ->
                    if wallet.Balance - amount > 0m then
                        do! Database.deductFromBalance environment.Connection customerId amount

                        let reply =
                            { SagaId = command.SagaId
                              OrderId = command.OrderId
                              Type = PaymentProcessed }

                        do! environment.Publish "payment-replies" orderId (Encode.reply reply |> Encode.toString 4)
                    else
                        let reply =
                            { SagaId = command.SagaId
                              OrderId = command.OrderId
                              Type = InsufficientFunds }

                        do! environment.Publish "payment-replies" orderId (Encode.reply reply |> Encode.toString 4)
            | Error e -> environment.Logger.LogError e
        }

    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            while not ct.IsCancellationRequested do
                environment.Logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)

                let result = environment.Consumer.Consume ct

                environment.Logger.LogInformation "Message received"

                do! handleMessage result.Message.Value
        }
