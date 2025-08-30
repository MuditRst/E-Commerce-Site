public class KafkaSettings
{
    public string Mode { get; set; } = "Azure";
    public string BootstrapServers { get; set; } = "";
    public string ConnectionString { get; set; } = "";
    public string Topic { get; set; } = "orders";
}
