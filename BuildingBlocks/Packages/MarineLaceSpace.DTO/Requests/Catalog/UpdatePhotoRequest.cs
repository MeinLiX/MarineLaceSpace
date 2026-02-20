namespace MarineLaceSpace.DTO.Requests.Catalog;

public class UpdatePhotoRequest
{
    public string? AltText { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsMain { get; set; }
}
