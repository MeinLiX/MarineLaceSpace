namespace MarineLaceSpace.DTO.Responses.Catalog;

public class ShopResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string UrlSlug { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}