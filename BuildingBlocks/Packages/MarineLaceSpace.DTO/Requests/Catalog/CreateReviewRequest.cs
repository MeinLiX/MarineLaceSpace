namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateReviewRequest
{
    public decimal Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? ContactInfo { get; set; }
}
