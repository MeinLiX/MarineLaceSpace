namespace MarineLaceSpace.DTO.Responses.Catalog;

public class CategoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ParentCategoryId { get; set; }
    public int Level { get; set; }
    public string? FullPath { get; set; }
    public List<CategoryResponse> Subcategories { get; set; } = [];
}
