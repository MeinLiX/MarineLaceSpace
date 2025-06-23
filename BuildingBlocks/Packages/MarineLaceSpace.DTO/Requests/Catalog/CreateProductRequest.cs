using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.DTO.Requests.Catalog;

public class CreateProductRequest
{
    [Required]
    public string Name { get; set; }

    [Required]
    public string Description { get; set; }

    [Required]
    public string CategoryId { get; set; }

    public bool AllowPersonalization { get; set; } = false;

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int Quantity { get; set; }

    public List<string>? Tags { get; set; }
    public List<string>? Materials { get; set; }
}