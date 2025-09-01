using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public enum OrderStatus
{
    Created = 0,
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Cancelled = 4
}

public class Orders
{
    [Key]
    [JsonPropertyName("id")]
    public string ID { get; set; } = Guid.NewGuid().ToString();

    public string Item { get; set; } = string.Empty;
    public int Quantity { get; set; }

    [JsonPropertyName("userId")] 
    public string UserId { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus OrderStatus { get; set; }
}
