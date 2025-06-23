using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.DTO.Requests.Catalog;

public class VariationImageAssociationRequest
{
    [Required]
    public string ImageId { get; set; }

    public string? SizeId { get; set; }
    public string? ColorId { get; set; }
    public string? MaterialId { get; set; }
}