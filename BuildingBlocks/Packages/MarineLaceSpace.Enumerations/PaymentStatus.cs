namespace MarineLaceSpace.Enumerations;

public class PaymentStatus(int id, string name) : Enumeration(id, name)
{
    public static readonly PaymentStatus Pending = new(1, "Pending");
    public static readonly PaymentStatus Succeeded = new(2, "Succeeded");
    public static readonly PaymentStatus Failed = new(3, "Failed");
    public static readonly PaymentStatus Refunded = new(4, "Refunded");
}
