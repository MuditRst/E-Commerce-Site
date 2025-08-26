using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


public class OrderStatusHistory()
{
    [Key]
    public int HistoryID { get; set; }

    [ForeignKey("Orders")]
    public int OrderID { get; set; }
    public Orders? Order { get; set; }

    public OrderStatus OrderStatus { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    public string? ChangedBy { get; set; }
}