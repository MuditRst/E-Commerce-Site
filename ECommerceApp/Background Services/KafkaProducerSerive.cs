using Confluent.Kafka;
using Microsoft.Extensions.Options;
using System.Text.Json;

public class KafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly KafkaSettings _settings;

    public KafkaProducerService(IOptions<KafkaSettings> kafkaOptions)
    {
        _settings = kafkaOptions.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = "$ConnectionString",
            SaslPassword = _settings.ConnectionString
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync<T>(T message, string? key = null)
    {
        var payload = JsonSerializer.Serialize(message);

        await _producer.ProduceAsync(_settings.Topic, new Message<string, string>
        {
            Key = key,
            Value = payload
        });

        Console.WriteLine($"✅ Message produced to topic {_settings.Topic}: {payload}");
    }
}
