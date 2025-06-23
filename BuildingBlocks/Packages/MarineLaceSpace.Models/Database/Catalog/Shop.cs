using MarineLaceSpace.Models.Database.Auth;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Database.Catalog;

public class Shop
{
    [Key]
    public string Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public required string OwnerId { get; set; }

    [ForeignKey("OwnerId")]
    public virtual AuthUser Owner { get; set; }

    [Required]
    [StringLength(100)]
    public required string UrlSlug { get; set; }

    public string? LogoUrl { get; set; }

    public string? BannerUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<Product> Products { get; set; } = [];
}