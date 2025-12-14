module Database

open System.Data
open Dapper
open Types

let get (connection: IDbConnection) (customerId: int) =
    task {
        let sql =
            """
            SELECT 
                id as "Id",
                customer_id as "CustomerId",
                balance as "Balance"
            FROM wallet
            WHERE customer_id = @customerId;
            """

        let! wallet = connection.QueryFirstOrDefaultAsync<WalletEntity>(sql, {| customerId = customerId |})

        match box wallet with
        | null -> return None
        | _ -> return Some wallet
    }

let deductFromBalance (connection: IDbConnection) (customerId: int) (amount: decimal) =
    task {
        let sql =
            """
            UPDATE wallet
            SET 
                balance = balance - @amount
            WHERE customer_id = @customerId
            AND balance >= @amount;
            """

        let! _ =
            connection.ExecuteAsync(
                sql,
                {| customerId = customerId
                   amount = amount |}
            )

        ()
    }
