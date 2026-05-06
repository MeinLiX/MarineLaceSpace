using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateShopRequest
{
    [Required]
    public string Name { get; set; }

    public string? UrlSlug { get; set; }

    public string? Description { get; set; }
}