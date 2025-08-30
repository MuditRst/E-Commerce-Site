using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaSettings _kafkaSettings;
    private readonly CosmosClient _cosmosClient;
    private Container _ordersContainer;
    private Container _logsContainer;

    public KafkaConsumerService(
        IServiceProvider serviceProvider,
        IOptions<KafkaSettings> kafkaOptions,
        CosmosClient cosmosClient)
    {
        _serviceProvider = serviceProvider;
        _kafkaSettings = kafkaOptions.Value;
        _cosmosClient = cosmosClient;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var db = await _cosmosClient.CreateDatabaseIfNotExistsAsync("ordersdb");
        var ordersResponse = await db.Database.CreateContainerIfNotExistsAsync("orders", "/userId");
        var logsResponse = await db.Database.CreateContainerIfNotExistsAsync("kafkalogs", "/topic");

        _ordersContainer = ordersResponse.Container;
        _logsContainer = logsResponse.Container;

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = "$ConnectionString",
            SaslPassword = _kafkaSettings.ConnectionString,
            GroupId = "order-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            BrokerVersionFallback = "0.10.0",
            ApiVersionFallbackMs = 15000
        };

        var topicName = _kafkaSettings.Topic ?? "orders";

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(topicName);

        Console.WriteLine($"Kafka Consumer started. Listening to topic {topicName}...");

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
                    await _logsContainer.CreateItemAsync(logDoc, new PartitionKey(logDoc.topic), cancellationToken: stoppingToken);

                    var order = JsonSerializer.Deserialize<Orders>(
                        result.Message.Value,
                        new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } }
                    );

                    if (order != null)
                    {
                        await _ordersContainer.UpsertItemAsync(order, new PartitionKey(order.UserId), cancellationToken: stoppingToken);
                        Console.WriteLine($"Order {order.ID} saved to CosmosDB (User {order.UserId})");
                    }

                    using var scope = _serviceProvider.CreateScope();
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
