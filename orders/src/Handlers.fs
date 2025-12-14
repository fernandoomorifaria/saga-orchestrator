module Handlers

open System
open Microsoft.AspNetCore.Http
open Giraffe
open Types

let createOrderHandler (createOrder: CreateOrder) (startSaga: StartSaga) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! request = ctx.BindJsonAsync<CreateOrderRequest>()

            let orderId = Guid.NewGuid()

            let order =
                { OrderId = orderId
                  State = "Pending"
                  CustomerId = request.CustomerId
                  ProductId = request.ProductId
                  Amount = request.Amount }

            let command =
                { OrderId = orderId
                  CustomerId = request.CustomerId
                  ProductId = request.ProductId
                  Amount = request.Amount }

            do! createOrder order

            do! startSaga command

            ctx.SetStatusCode 201

            return! text (orderId.ToString()) next ctx
        }

let getOrderHandler (getOrder: GetOrder) (orderId: Guid) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! order = getOrder orderId

            match order with
            | Some order -> return! json order next ctx
            | None ->
                ctx.SetStatusCode 404
                return! next ctx
        }
