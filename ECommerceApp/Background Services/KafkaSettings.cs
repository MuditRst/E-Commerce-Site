public class KafkaSettings
{
    public string Mode { get; set; } = "Azure";
    public string BootstrapServers { get; set; } = "";
    public string Topic { get; set; } = "orders";
    public string ConsumerGroupId { get; set; } = "order-consumer-group";
    public string ConnectionString => Environment.GetEnvironmentVariable("EVENTHUB_CONNECTION_STRING") ?? "";
}
