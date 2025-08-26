using System.ComponentModel.DataAnnotations;

public class KafkaLog
{
    [Key]
    public int LogID { get; set; }

    public string Topic { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
