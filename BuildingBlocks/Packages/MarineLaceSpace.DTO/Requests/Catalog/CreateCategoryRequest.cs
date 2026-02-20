namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentCategoryId { get; set; }
}
