namespace MarineLaceSpace.Models.Auth;

public class RefreshToken
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(7);
    public bool IsRevoked { get; set; } = false;
    public bool IsUsed { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual AuthUser User { get; set; }
}
