using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class KafkaLog
{
    [Key]
    [JsonPropertyName("id")] 
    public string ID { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("topic")] 
    public string Topic { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
