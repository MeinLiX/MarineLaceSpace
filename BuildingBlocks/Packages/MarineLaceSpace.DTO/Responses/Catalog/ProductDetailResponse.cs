namespace MarineLaceSpace.DTO.Responses.Catalog;

public class ProductDetailResponse : ProductSummaryResponse
{
    public string? Description { get; set; }
    public int TotalQuantity { get; set; } 
    public bool AllowPersonalization { get; set; }
    public List<string>? Tags { get; set; }
    public List<string>? Materials { get; set; }
    public List<ProductPhotoResponse> Photos { get; set; } = [];
    public string ShopId { get; set; }
    public List<ProductInventoryItemResponse> Inventory { get; set; } = [];
}