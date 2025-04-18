using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Auth;

public class RefreshToken
{
    [Key]
    public string Id { get; set; }

    [Required]
    public string Token { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(7);

    [Required]
    public bool IsRevoked { get; set; } = false;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("User")]
    public string UserId { get; set; }
    public AuthUser User { get; set; }
}
