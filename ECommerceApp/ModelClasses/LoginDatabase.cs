using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class LoginDatabase
{
    [Key]
    [JsonPropertyName("id")]
    public string ID { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [JsonPropertyName("username")] 
    public string Username { get; set; } = string.Empty;

    [Required]
    [JsonIgnore]
    public string Password { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public ICollection<Orders>? Orders { get; set; }
}
