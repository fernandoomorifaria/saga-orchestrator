namespace Orchestrator

open System
open System.Collections.Generic
open System.Data
open System.Text.Json
open System.Text.Json.Serialization
open System.Linq
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Confluent.Kafka
open Types

type Worker
    (
        producer: IProducer<string, string>,
        consumer: IConsumer<string, string>,
        connection: IDbConnection,
        logger: ILogger<Worker>
    ) =
    inherit BackgroundService()

    let publish (topic: string) (message: SagaMessage) =
        task {
            let key = message.Order.OrderId.ToString()

            let json = JsonSerializer.Serialize message

            let! _ = producer.ProduceAsync(topic, Message<string, string>(Key = key, Value = json))

            ()
        }

    let handleOrder (message: string) =
        task {
            let order = JsonSerializer.Deserialize<Order> message

            logger.LogInformation("Order {id} received", order.OrderId)

            let sagaId = Guid.NewGuid()

            let saga =
                { SagaId = sagaId
                  State = "Pending"
                  Order = order }

            do! Database.create connection saga

            let command =
                { SagaId = sagaId
                  State = saga.State
                  Type = "ReserveInvetory"
                  Order = order }

            (* TODO: Move to it's own function *)
            do! publish "inventory" command
        }

    let handlePayment (message: string) =
        task {
            let sagaMessage = JsonSerializer.Deserialize<SagaMessage> message

            match sagaMessage.Type with
            | "PaymentProcessed" -> ()
            | "InsufficientFunds" -> ()
        }

    let processPayment (message: SagaMessage) =
        task { do! publish "payments" { message with Type = "ProcessPayment" } }

    let handleInventory (message: string) =
        task {
            let sagaMessage = JsonSerializer.Deserialize<SagaMessage> message

            logger.LogInformation("{type} event received", sagaMessage.Type)

            match sagaMessage.Type with
            | "InventoryReserved" -> do! processPayment sagaMessage
            | "OutOfStock" -> ()
        }

    let handleMessage (topic: string) (message: string) =
        task {
            (* TODO: JsonConverter to ADT? *)
            match topic with
            | "orders" -> do! handleOrder message
            | "inventory" -> do! handleInventory message
            | _ -> ()
        }

    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            while not ct.IsCancellationRequested do
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)

                let result = consumer.Consume(ct)

                handleMessage result.Topic result.Message.Value |> ignore
        }
