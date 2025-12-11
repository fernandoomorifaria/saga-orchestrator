module Database

open System
open System.Data
open System.Text.Json
open System.Threading.Tasks
open Dapper
open Types

let get (connection: IDbConnection) (sagaId: Guid) : Task<Saga option> =
    task {
        let sql =
            """
            SELECT 
                id AS "Id",
                saga_id AS "SagaId",
                state AS "State",
                current_step AS "CurrentStep",
                "order"::text AS "Order",
                created_at AS "CreatedAt",
                last_updated_at AS "LastUpdatedAt"
            FROM saga
            WHERE saga_id = @sagaId
            """

        let! saga = connection.QuerySingleOrDefaultAsync<SagaEntity>(sql, {| sagaId = sagaId |})

        (* NOTE: I know, this looks bad *)
        match box saga with
        | null -> return None
        | _ ->
            return
                Some
                    { SagaId = saga.SagaId
                      State = saga.State
                      CurrentStep = saga.CurrentStep
                      Order = JsonSerializer.Deserialize<Order> saga.Order }
    }

let create (connection: IDbConnection) (saga: Saga) =
    task {
        let sql =
            """
            INSERT INTO saga (
                saga_id,
                state,
                current_step,
                "order"
            )
            VALUES (
                @sagaId,
                @state,
                @currentStep,
                @order::jsonb
            );
            """

        let! _ =
            connection.ExecuteAsync(
                sql,
                {| sagaId = saga.SagaId
                   state = saga.State
                   currentStep = saga.CurrentStep
                   order = JsonSerializer.Serialize saga.Order |}
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
                current_step = @currentStep,
                "order" = @order::jsonb,
                last_updated_at = NOW()
            WHERE
                saga_id = @sagaId;
          """

        let! _ =
            connection.ExecuteAsync(
                sql,
                {| sagaId = saga.SagaId
                   state = saga.State
                   currentStep = saga.CurrentStep
                   order = JsonSerializer.Serialize saga.Order |}
            )

        ()
    }
