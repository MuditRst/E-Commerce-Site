using System.ComponentModel.DataAnnotations;

public class Orders
{
    [Key]
    public int OrderID { get; set; }
    public string Item { get; set; } = string.Empty;
    public int Quantity { get; set; }
}