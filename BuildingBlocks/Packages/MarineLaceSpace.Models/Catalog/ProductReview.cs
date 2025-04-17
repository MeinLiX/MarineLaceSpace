using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Catalog;

// Відгуки на товар
public class ProductReview
{
    [Key]
    public string Id { get; set; }

    public string ProductId { get; set; }
    public Product Product { get; set; }

    // Рейтинг (від 0 до 5 з кроком 0.5)
    public decimal Rating { get; set; }

    [StringLength(1000)]
    public string Comment { get; set; }

    public string UserId { get; set; }

    [StringLength(100)]
    public string UserName { get; set; }

    [StringLength(200)]
    public string ContactInfo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsVerified { get; set; } = false;
}
