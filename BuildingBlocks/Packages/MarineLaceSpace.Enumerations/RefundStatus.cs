namespace MarineLaceSpace.Enumerations;

public class RefundStatus(int id, string name) : Enumeration(id, name)
{
    public static readonly RefundStatus Pending = new(1, "Pending");
    public static readonly RefundStatus Approved = new(2, "Approved");
    public static readonly RefundStatus Completed = new(3, "Completed");
    public static readonly RefundStatus Rejected = new(4, "Rejected");
}
