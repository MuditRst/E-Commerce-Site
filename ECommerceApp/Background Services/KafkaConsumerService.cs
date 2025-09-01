using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaSettings _kafkaSettings;

    public KafkaConsumerService(
        IServiceProvider serviceProvider,
        IOptions<KafkaSettings> kafkaOptions)
    {
        _serviceProvider = serviceProvider;
        _kafkaSettings = kafkaOptions.Value;
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
            GroupId = _kafkaSettings.ConsumerGroupId ?? "order-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
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

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DBContext>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<OrderHub>>();

                    // Save KafkaLog
                    var log = new KafkaLog
                    {
                        ID = Guid.NewGuid().ToString(),
                        Topic = result.Topic,
                        Message = result.Message.Value,
                        Timestamp = DateTime.UtcNow
                    };
                    db.KafkaLogs.Add(log);

                    // Deserialize and save Order
                    var order = JsonSerializer.Deserialize<Orders>(
                        result.Message.Value,
                        new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } }
                    );

                    if (order != null)
                    {
                        db.Orders.Update(order);
                        Console.WriteLine($"Order {order.ID} saved to CosmosDB (User {order.UserId})");
                    }

                    await db.SaveChangesAsync(stoppingToken);

                    // Broadcast to clients
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
