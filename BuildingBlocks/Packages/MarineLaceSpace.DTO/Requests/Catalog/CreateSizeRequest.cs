namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateSizeRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCustom { get; set; }
    public int GenderId { get; set; } = 3; // Unisex default
}
