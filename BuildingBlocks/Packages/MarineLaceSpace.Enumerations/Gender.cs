namespace MarineLaceSpace.Enumerations;

public class Gender(int id, string name) : Enumeration(id, name)
{
    public static readonly Gender Male = new(1, "Male");
    public static readonly Gender Famele = new(2, "Famele");
    public static readonly Gender NotDisclosed = new(3, "Not disclosed");
    public static readonly Gender Other = new(4, "Other");
}
