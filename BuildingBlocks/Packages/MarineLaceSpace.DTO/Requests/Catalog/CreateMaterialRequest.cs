namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
