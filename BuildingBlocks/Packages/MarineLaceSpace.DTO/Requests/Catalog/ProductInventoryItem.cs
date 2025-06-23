namespace MarineLaceSpace.DTO.Requests.Catalog;

public class ProductInventoryItem
{
    public string? SizeId { get; set; }
    public string? ColorId { get; set; }
    public string? MaterialId { get; set; }

    public decimal Price { get; set; }
    public int Quantity { get; set; }
}