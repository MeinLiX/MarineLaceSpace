using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Basket;

public class BasketEntity
{
    [Key]
    public string BuyerId { get; set; } = string.Empty;

    public string ItemsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
