module Database

open System
open System.Data
open System.Threading.Tasks
open Dapper
open Types
open Thoth.Json.Net

let get (connection: IDbConnection) (sagaId: Guid) : Task<Saga option> =
    task {
        let sql =
            """
            SELECT 
                id AS "Id",
                saga_id AS "SagaId",
                state AS "State",
                "order"::text AS "Order",
                created_at AS "CreatedAt",
                last_updated_at AS "LastUpdatedAt"
            FROM saga
            WHERE saga_id = @sagaId
            """

        let! saga = connection.QuerySingleOrDefaultAsync<SagaEntity>(sql, {| sagaId = sagaId |})

        match box saga with
        | null -> return None
        | _ ->
            let order = Decode.fromString Decode.order saga.Order
            let state = Decode.state saga.State

            match order with
            | Ok order ->
                return
                    Some
                        { SagaId = saga.SagaId
                          State = state
                          Order = order }
            | Error err -> return failwith $"Failed to decode order: {err}"
    }

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
                @sagaId,
                @state,
                @order::jsonb
            );
            """

        let! _ =
            connection.ExecuteAsync(
                sql,
                {| sagaId = saga.SagaId
                   state = Encode.state saga.State
                   order = Encode.order saga.Order |> Encode.toString 4 |}
            )

        ()
    }

let update (connection: IDbConnection) (saga: Saga) =
    task {
        let sql =
            """
            UPDATE saga
            SET
                state = @state,
                "order" = @order::jsonb,
                last_updated_at = NOW()
            WHERE
                saga_id = @sagaId;
          """

        let! _ =
            connection.ExecuteAsync(
                sql,
                {| sagaId = saga.SagaId
                   state = Encode.state saga.State
                   order = Encode.order saga.Order |> Encode.toString 4 |}
            )

        ()
    }
