using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateShopRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string UrlSlug { get; set; }

    public string? Description { get; set; }
}