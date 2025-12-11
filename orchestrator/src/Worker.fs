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

    let publish (topic: string) (key: string) message =
        task {
            let json = JsonSerializer.Serialize message

            let! _ = producer.ProduceAsync(topic, Message<string, string>(Key = key, Value = json))

            ()
        }
    (* NOTE: Maybe the old way was better *)
    let steps: SagaStep array =
        [| { Name = "ReserveInventory"
             CommandTopic = "inventory"
             ReplyTopic = "inventory-replies"
             CompensationCommand = "ReleaseInventory" }
           { Name = "ProcessPayment"
             CommandTopic = "payments"
             ReplyTopic = "payments-replies"
             CompensationCommand = "RefundPayment" } |]

    let completeSaga (saga: Saga) = task { }

    (* NOTE: Is there a better way to use ADT here? *)
    let getCommand (command: string) (saga: Saga) : Command =
        match command with
        | "ReserveInventory" ->
            ReserveInventory
                { SagaId = saga.SagaId
                  OrderId = saga.Order.OrderId
                  Type = "ReserveInventory"
                  ProductId = saga.Order.ProductId }
        | "ProcessPayment" ->
            ProcessPayment
                { SagaId = saga.SagaId
                  OrderId = saga.Order.OrderId
                  Type = "ProcessPayment"
                  CustomerId = saga.Order.CustomerId
                  Amount = saga.Order.Amount }

    (* NOTE: If event success is false call compensate instead of transition *)
    let transition (saga: Saga) =
        task {
            if saga.CurrentStep >= steps.Length then
                do! completeSaga saga
            else
                let step = steps.[saga.CurrentStep]

                let command = getCommand step.Name saga

                do! publish step.CommandTopic (saga.SagaId.ToString()) command

                ()
        }

    let handleReply (event: Event) =
        task {
            let! saga = Database.get connection event.SagaId

            match saga with
            | None -> ()
            | Some saga ->
                if event.Success = true then
                    let next =
                        { saga with
                            CurrentStep = saga.CurrentStep + 1 }

                    do! Database.update connection next

                    do! transition next
                else
                    (* TODO: Compensate *)
                    ()
        }

    let startSaga (order: Order) =
        task {
            logger.LogInformation("Order {id} received", order.OrderId)

            let sagaId = Guid.NewGuid()

            let saga =
                { SagaId = sagaId
                  State = "Pending"
                  CurrentStep = 0
                  Order = order }

            do! Database.create connection saga

            do! transition saga
        }

    let handleMessage (topic: string) (message: string) =
        task {
            if topic = "orders" then
                let order = JsonSerializer.Deserialize<Order> message
                do! startSaga order
            else
                let event = JsonSerializer.Deserialize<Event> message

                do! handleReply event
        }

    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            while not ct.IsCancellationRequested do
                logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now)

                let result = consumer.Consume(ct)

                handleMessage result.Topic result.Message.Value |> ignore
        }
