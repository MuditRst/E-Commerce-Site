using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class LoginDatabase
{
    [Key]
    public int UserID { get; set; }
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    [Required]
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "User";
    
    public ICollection<Orders>? Orders { get; set; }
}