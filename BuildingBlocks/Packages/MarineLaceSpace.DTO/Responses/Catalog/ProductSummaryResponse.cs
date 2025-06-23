namespace MarineLaceSpace.DTO.Responses.Catalog;

public class ProductSummaryResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string? MainImageUrl { get; set; }
}