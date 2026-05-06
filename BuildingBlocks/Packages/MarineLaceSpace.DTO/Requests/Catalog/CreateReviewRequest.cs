namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateReviewRequest
{
    public decimal Rating { get; set; }
    public string? Title { get; set; }
    public string Text { get; set; } = string.Empty;
    /// <summary>Fallback: legacy field mapped to Text if Text is empty.</summary>
    public string? Comment { get; set; }
    public string? UserName { get; set; }
    public string? ContactInfo { get; set; }
}
