namespace Payments

open System
open System.Collections.Generic
open System.Linq
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Confluent.Kafka
open Thoth.Json.Net
open Types

type Worker(environment: Environment) =
    inherit BackgroundService()

    let publish (topic: string) (key: string) (message: string) =
        task {
            // logger.LogInformation("Publishing {} to {topic}", message, topic)

            let! _ = environment.Producer.ProduceAsync(topic, Message<string, string>(Key = key, Value = message))

            ()
        }

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
                    if (wallet.Balance - amount) > 0 then
                        do! Database.deductFromBalance environment.Connection customerId amount

                        let reply =
                            { SagaId = command.SagaId
                              OrderId = command.OrderId
                              Type = PaymentProcessed }

                        do! publish "payment-replies" orderId (Encode.reply reply |> Encode.toString 4)
                    else
                        let reply =
                            { SagaId = command.SagaId
                              OrderId = command.OrderId
                              Type = InsufficientFunds }

                        do! publish "payment-replies" orderId (Encode.reply reply |> Encode.toString 4)
            | Error e -> ()
        }

    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            while not ct.IsCancellationRequested do
                // logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)

                let result = environment.Consumer.Consume ct

                do! handleMessage result.Message.Value
        }
