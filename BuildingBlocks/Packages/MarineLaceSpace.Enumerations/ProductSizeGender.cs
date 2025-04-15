namespace MarineLaceSpace.Enumerations;

public class ProductSizeGender(int id, string name) : Enumeration(id, name)
{
    public static readonly ProductSizeGender Male = new(1, "Male");
    public static readonly ProductSizeGender Famele = new(2, "Famele");
    public static readonly ProductSizeGender Unisex = new(3, "Unisex");
}
