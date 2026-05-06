namespace MarineLaceSpace.DTO.Responses.Catalog;

public class ReviewSummaryResponse
{
    public decimal AverageRating { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<int, int> Distribution { get; set; } = new();
}
