namespace MarineLaceSpace.DTO.Responses.Catalog;

public class ReviewResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsVerified { get; set; }
}
