namespace MarineLaceSpace.DTO.Responses.Catalog;

public class ReviewResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string? Title { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? GuestName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVerifiedPurchase { get; set; }
}
