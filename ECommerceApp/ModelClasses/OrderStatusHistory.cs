using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;


public class OrderStatusHistory()
{
    [Key]
    [JsonPropertyName("id")] 
    public string ID { get; set; } = Guid.NewGuid().ToString();

    [ForeignKey("Order")]
    [JsonPropertyName("orderId")] 
    public string OrderID { get; set; } = string.Empty;
    public Orders? Order { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public OrderStatus OrderStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? ChangedBy { get; set; }
}