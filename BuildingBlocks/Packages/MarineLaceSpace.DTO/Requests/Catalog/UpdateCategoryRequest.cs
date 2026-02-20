namespace MarineLaceSpace.DTO.Requests.Catalog;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
