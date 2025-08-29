using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly CosmosClient cosmosClient;
    private Container ordersContainer;
    private Container logsContainer;

    public KafkaConsumerService(IServiceProvider serviceProvider, IConfiguration configuration, CosmosClient cosmosClient)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.cosmosClient = cosmosClient;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var db = await cosmosClient.CreateDatabaseIfNotExistsAsync("ordersdb");
        var ordersResponse = await db.Database.CreateContainerIfNotExistsAsync("orders", "/userId");
        var logsResponse = await db.Database.CreateContainerIfNotExistsAsync("kafkalogs", "/topic");

        ordersContainer = ordersResponse.Container;
        logsContainer = logsResponse.Container;

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = "order-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(configuration["Kafka:Topic"] ?? "order-topic");

        Console.WriteLine("Kafka Consumer started. Listening to topic...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    Console.WriteLine($"Received: {result.Message.Value}");

                    var logDoc = new
                    {
                        id = Guid.NewGuid().ToString(),
                        topic = result.Topic,
                        message = result.Message.Value,
                        timestamp = DateTime.UtcNow
                    };
                    await logsContainer.CreateItemAsync(logDoc, new PartitionKey(logDoc.topic), cancellationToken: stoppingToken);

                    var order = JsonSerializer.Deserialize<Orders>(
                        result.Message.Value,
                        new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } }
                    );

                    if (order != null)
                    {
                        order.ID = order.ID;
                        order.UserId = order.UserId;

                        await ordersContainer.UpsertItemAsync(order, new PartitionKey(order.UserId), cancellationToken: stoppingToken);
                        Console.WriteLine($"Order {order.ID} saved to CosmosDB (User {order.UserId})");
                    }

                    using var scope = serviceProvider.CreateScope();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<OrderHub>>();
                    await hubContext.Clients.All.SendAsync("ReceiveOrder", result.Message.Value, cancellationToken: stoppingToken);
                }
                catch (ConsumeException ce)
                {
                    Console.WriteLine($"Kafka consume error: {ce.Error.Reason}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            consumer.Close();
        }
    }
}
