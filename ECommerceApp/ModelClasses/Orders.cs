using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Orders
{
    [Key]
    public int OrderID { get; set; }
    public string Item { get; set; } = string.Empty;
    public int Quantity { get; set; }

    [ForeignKey("User")]
    public int UserID { get; set; }

    public LoginDatabase? User { get; set; }
}