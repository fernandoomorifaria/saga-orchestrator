module Database

open System.Data
open System.Text.Json
open Dapper
open Types

let create (connection: IDbConnection) (saga: Saga) =
    task {
        let sql =
            """
          INSERT INTO saga (
              saga_id,
              state,
              "order"
          )
          VALUES (
              @SagaId,
              @State,
              @Order::jsonb
          );
          """

        let! _ =
            connection.ExecuteAsync(
                sql,
                {| SagaId = saga.SagaId
                   State = saga.State
                   Order = JsonSerializer.Serialize saga.Order |}
            )

        ()
    }
