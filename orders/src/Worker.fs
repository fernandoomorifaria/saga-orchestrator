namespace Orders

open System.Text.Json
open System.Threading
open Microsoft.Extensions.Hosting
open Types

type Worker(environment: Environment) =
    inherit BackgroundService()

    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            while not ct.IsCancellationRequested do
                let result = environment.Consumer.Consume ct

                let event = JsonSerializer.Deserialize<Event> result.Message.Value

                if event.Type = "OrderPlaced" then
                    let! order = environment.GetOrder event.OrderId

                    let update =
                        { order.Value with
                            State = "OrderPlaced" }

                    do! environment.UpdateOrder update
        }
