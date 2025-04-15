namespace MarineLaceSpace.Models.Catalog;

// Зв'язкова таблиця між товаром і матеріалом з інформацією про ціну
public class ProductMaterial
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int MaterialId { get; set; }
    public Material Material { get; set; }

    // Модифікатор ціни для цього матеріалу
    public decimal PriceModifier { get; set; } = 0;
}
