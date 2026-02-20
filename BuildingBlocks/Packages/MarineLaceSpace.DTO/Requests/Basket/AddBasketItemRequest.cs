namespace MarineLaceSpace.DTO.Requests.Basket;

public class AddBasketItemRequest
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SizeId { get; set; }
    public string? SizeName { get; set; }
    public string? ColorId { get; set; }
    public string? ColorName { get; set; }
    public string? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Personalization { get; set; }
    public string? ImageUrl { get; set; }
}
