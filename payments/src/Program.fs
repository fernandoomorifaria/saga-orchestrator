namespace Payments

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Confluent.Kafka
open Npgsql
open Types

module CompositionRoot =
    let private createProducer (configuration: IConfiguration) =
        let server = configuration.["Kafka:BootstrapServer"]
        let config = ProducerConfig(BootstrapServers = server)

        ProducerBuilder<string, string>(config).Build()

    let private createConsumer (configuration: IConfiguration) =
        let server = configuration.["Kafka:BootstrapServer"]
        let config = ConsumerConfig(BootstrapServers = server, GroupId = "payment-consumer")

        let consumer = ConsumerBuilder<string, string>(config).Build()

        consumer.Subscribe [| "payments" |]

        consumer

    let compose (configuration: IConfiguration) : Environment =
        let connectionString = configuration.GetConnectionString "Default"
        let dataSource = NpgsqlDataSource.Create connectionString
        let connection = dataSource.CreateConnection()

        let producer = createProducer configuration
        let consumer = createConsumer configuration

        let loggerFactory =
            LoggerFactory.Create(fun builder -> builder.AddConsole() |> ignore)

        let logger = loggerFactory.CreateLogger<Worker>()

        let publish (topic: string) (key: string) (message: string) =
            task {
                let! _ = producer.ProduceAsync(topic, Message<string, string>(Key = key, Value = message))

                ()
            }

        { Publish = publish
          Consumer = consumer
          Connection = connection
          Logger = logger }

module Program =
    [<EntryPoint>]
    let main args =
        let builder = Host.CreateApplicationBuilder args

        let environment = CompositionRoot.compose builder.Configuration

        builder.Services.AddHostedService(fun _ -> new Worker(environment)) |> ignore

        builder.Build().Run()

        0
