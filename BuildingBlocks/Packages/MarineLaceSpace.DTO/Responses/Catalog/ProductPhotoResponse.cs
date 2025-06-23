namespace MarineLaceSpace.DTO.Responses.Catalog;

public class ProductPhotoResponse
{
    public string Id { get; set; }
    public string Url { get; set; }
    public bool IsMain { get; set; }

    public string? SizeId { get; set; }
    public string? ColorId { get; set; }
    public string? MaterialId { get; set; }
}