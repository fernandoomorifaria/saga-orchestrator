namespace Orders

open System.Threading
open Microsoft.Extensions.Hosting
open Thoth.Json.Net
open Types

type Worker(environment: Environment) =
    inherit BackgroundService()

    override _.ExecuteAsync(ct: CancellationToken) =
        task {
            while not ct.IsCancellationRequested do
                let result = environment.Consumer.Consume ct

                let event = Decode.fromString Decode.orderEvent result.Message.Value

                match event with
                | Ok event ->
                    let! order = environment.GetOrder event.OrderId

                    match order with
                    | Some order ->
                        let status = Encode.status event.Status

                        let updatedOrder = { order with State = status }

                        do! environment.UpdateOrder updatedOrder
                    | None -> ()
                | Error e -> ()
        }
