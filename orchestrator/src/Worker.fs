namespace Orchestrator

open System
open System.Collections.Generic
open System.Data
open System.Linq
open System.Threading
open System.Threading.Tasks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Confluent.Kafka
open Types
open Thoth.Json.Net

type Worker
    (
        producer: IProducer<string, string>,
        consumer: IConsumer<string, string>,
        connection: IDbConnection,
        logger: ILogger<Worker>
    ) =
    inherit BackgroundService()

    let publish (topic: string) (key: string) (message: string) =
        task {
            logger.LogInformation("Publishing {} to {topic}", message, topic)

            let! _ = producer.ProduceAsync(topic, Message<string, string>(Key = key, Value = message))

            ()
        }

    let complete (saga: Saga) =
        task {
            do! Database.update connection { saga with State = Completed }

            logger.LogInformation("Saga {id} completed", saga.SagaId)

            let key = saga.Order.OrderId.ToString()

            let event =
                { OrderId = saga.Order.OrderId
                  Status = Placed }

            do! publish "order-replies" key (Encode.orderEvent event |> Encode.toString 4)
        }

    let fail (saga: Saga) =
        task {
            do! Database.update connection { saga with State = Failed }

            logger.LogInformation("Saga {id} failed", saga.SagaId)

            let key = saga.Order.OrderId.ToString()

            let event =
                { OrderId = saga.Order.OrderId
                  Status = Cancelled }

            do! publish "order-replies" key (Encode.orderEvent event |> Encode.toString 4)
        }

    let processPayment (saga: Saga) =
        task {
            let key = saga.Order.OrderId.ToString()

            let command =
                ProcessPaymentCommand
                    { SagaId = saga.SagaId
                      OrderId = saga.Order.OrderId
                      CustomerId = saga.Order.CustomerId
                      Amount = saga.Order.Amount
                      Type = ProcessPayment }

            do! publish "payments" key (Encode.command command |> Encode.toString 4)
        }

    let releaseInventory (saga: Saga) =
        task {
            let key = saga.Order.OrderId.ToString()
            let order = saga.Order

            let command =
                ReleaseInventoryCommand
                    { SagaId = saga.SagaId
                      OrderId = order.OrderId
                      ProductId = order.ProductId
                      Type = ReleaseInventory }

            do! publish "inventory" key (Encode.command command |> Encode.toString 4)
        }

    let handleReply (reply: Reply) =
        task {
            logger.LogInformation "Getting saga for reply"

            let! saga = Database.get connection reply.SagaId

            match saga with
            | None -> ()
            | Some saga ->
                match reply.Type with
                | InventoryReserved -> do! processPayment saga
                | OutOfStock -> do! fail saga
                | InventoryReleased -> do! fail saga
                | PaymentProcessed -> do! complete saga
                | InsufficientFunds -> do! releaseInventory saga
        }

    let startSaga (order: Order) =
        task {
            let sagaId = Guid.NewGuid()

            let saga =
                { SagaId = sagaId
                  State = Pending
                  Order = order }

            logger.LogInformation "Creating Saga"

            do! Database.create connection saga

            let order = saga.Order

            let command =
                ReserveInventoryCommand
                    { SagaId = saga.SagaId
                      OrderId = order.OrderId
                      ProductId = order.ProductId
                      Type = ReserveInventory }

            let key = order.OrderId.ToString()

            logger.LogInformation("Starting Saga {sagaId}", saga.SagaId)

            do! publish "inventory" key (Encode.command command |> Encode.toString 4)
        }

    let handleMessage (topic: string) (message: string) =
        task {
            if topic = "order-requests" then
                let order = Decode.fromString Decode.order message

                match order with
                | Ok order ->
                    logger.LogInformation("Order {id} received", order.OrderId)

                    do! startSaga order
                | Error e -> logger.LogInformation e
            else
                let reply = Decode.fromString Decode.reply message

                match reply with
                | Ok reply ->
                    logger.LogInformation("Received {type} reply", reply.Type)

                    do! handleReply reply
                | Error e -> logger.LogInformation e
        }

    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            while not ct.IsCancellationRequested do
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)

                let result = consumer.Consume ct

                logger.LogInformation("Message received {message}", result.Message.Value)

                handleMessage result.Topic result.Message.Value |> ignore
        }
