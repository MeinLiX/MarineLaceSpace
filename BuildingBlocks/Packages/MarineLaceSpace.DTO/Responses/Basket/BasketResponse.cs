namespace MarineLaceSpace.DTO.Responses.Basket;

public class BasketResponse
{
    public string BuyerId { get; set; } = string.Empty;
    public List<BasketItemResponse> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(i => i.UnitPrice * i.Quantity);
}

public class BasketItemResponse
{
    public string ItemId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? SizeId { get; set; }
    public string? SizeName { get; set; }
    public string? ColorId { get; set; }
    public string? ColorName { get; set; }
    public string? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Personalization { get; set; }
    public string? ImageUrl { get; set; }
}
