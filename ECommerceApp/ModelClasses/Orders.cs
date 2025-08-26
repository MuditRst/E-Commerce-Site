using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


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
    public int OrderID { get; set; }
    public string Item { get; set; } = string.Empty;
    public int Quantity { get; set; }

    [ForeignKey("User")]
    public int UserID { get; set; }
    public LoginDatabase? User { get; set; }
    public OrderStatus OrderStatus { get; set; }
}