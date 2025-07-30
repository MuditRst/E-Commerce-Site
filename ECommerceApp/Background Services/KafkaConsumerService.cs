using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR;

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
                    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<OrderHub>>();

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
