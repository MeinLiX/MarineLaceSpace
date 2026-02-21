namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateColorRequest
{
    public string Name { get; set; } = string.Empty;
    public string? HexCode { get; set; }
}
