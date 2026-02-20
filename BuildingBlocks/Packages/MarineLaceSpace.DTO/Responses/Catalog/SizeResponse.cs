namespace MarineLaceSpace.DTO.Responses.Catalog;

public class SizeResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCustom { get; set; }
    public string Gender { get; set; } = string.Empty;
}
