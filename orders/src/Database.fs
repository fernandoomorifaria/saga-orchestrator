module Database

open System
open System.Data
open Dapper
open Types

let get (connection: IDbConnection) (orderId: Guid) =
    task {
        let sql =
            """
            SELECT
                order_id AS "OrderId",
                state AS "State",
                customer_id AS "CustomerId",
                product_id  AS "ProductId",
                amount AS "Amount"
            FROM orders
            WHERE order_id = @orderId;
            """

        let! order = connection.QuerySingleOrDefaultAsync<Order>(sql, {| orderId = orderId |})

        match box order with
        | null -> return None
        | _ -> return Some order
    }

let create (connection: IDbConnection) (order: Order) =
    task {
        let sql =
            """
            INSERT INTO orders (
                order_id,
                state,
                customer_id,
                product_id,
                amount
            )
            VALUES (
                @orderId,
                @state,
                @customerId,
                @productId,
                @amount
            )
            """

        let! _ =
            connection.ExecuteAsync(
                sql,
                {| orderId = order.OrderId
                   state = order.State
                   customerId = order.CustomerId
                   productId = order.ProductId
                   amount = order.Amount |}
            )

        ()
    }

let update (connection: IDbConnection) (order: Order) =
    task {
        let sql =
            """
            UPDATE orders
            SET
                state = @state,
                customer_id = @customerId,
                product_id = @productId,
                amount = @amount,
                last_updated_at = NOW()
            WHERE order_id = @orderId;
            """

        let! _ =
            connection.ExecuteAsync(
                sql,
                {| orderId = order.OrderId
                   state = order.State
                   customerId = order.CustomerId
                   productId = order.ProductId
                   amount = order.Amount |}
            )

        ()
    }
