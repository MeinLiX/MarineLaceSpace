namespace MarineLaceSpace.DTO.Requests.Catalog;

public class UpdateProductInventoryRequest
{
    public List<ProductInventoryItem> InventoryItems { get; set; } = [];
}