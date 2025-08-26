using System.Data.Common;
using System.Text.Json;
using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

public class KafkaConsumerService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;

    public KafkaConsumerService(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();
        try
        {
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "order-consumer-group",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            await Task.Run(() => consumer.Subscribe("order-topic"),cancellationToken);

            Console.WriteLine("Kafka Consumer started. Listening to 'order-topic'...");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(cancellationToken);
                    Console.WriteLine($"Received: {result.Message.Value}");

                    using var scope = serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<DBContext>();
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<OrderHub>>();

                    db.KafkaLogs.Add(new KafkaLog
                    {
                        Topic = result.Topic,
                        Message = result.Message.Value,
                        Timestamp = DateTime.UtcNow   
                    });

                    var order = JsonSerializer.Deserialize<Orders>(result.Message.Value);
                    if (order != null)
                    {
                        order.User = null;
                        if (await db.Logins.AnyAsync(u => u.UserID == order.UserID, cancellationToken: cancellationToken))
                        {
                            var existing = await db.Orders.FirstOrDefaultAsync(o => o.OrderID == order.OrderID, cancellationToken);
                            if (existing == null)
                            {
                                db.Orders.Add(order);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Skipping order {order.OrderID} - UserID {order.UserID} not found");
                        }
                    }

                    await db.SaveChangesAsync(cancellationToken);

                    await hubContext.Clients.All.SendAsync("ReceiveOrder", result.Message.Value, cancellationToken: cancellationToken);
                }
                catch (ConsumeException ce)
                {
                    Console.WriteLine($"Kafka consume error: {ce.Error.Reason}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Kafka Error: {ex.Message}");
        }
    }
}
