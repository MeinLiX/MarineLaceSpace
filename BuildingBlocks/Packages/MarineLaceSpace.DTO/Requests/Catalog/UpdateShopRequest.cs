using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.DTO.Requests.Catalog;

public class UpdateShopRequest
{
    [Required]
    public string Name { get; set; }

    public string? Description { get; set; }
}