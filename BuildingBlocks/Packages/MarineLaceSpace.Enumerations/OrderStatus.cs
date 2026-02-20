namespace MarineLaceSpace.Enumerations;

public class OrderStatus(int id, string name) : Enumeration(id, name)
{
    public static readonly OrderStatus New = new(1, "New");
    public static readonly OrderStatus PendingPayment = new(2, "PendingPayment");
    public static readonly OrderStatus Paid = new(3, "Paid");
    public static readonly OrderStatus Processing = new(4, "Processing");
    public static readonly OrderStatus Shipped = new(5, "Shipped");
    public static readonly OrderStatus Delivered = new(6, "Delivered");
    public static readonly OrderStatus Completed = new(7, "Completed");
    public static readonly OrderStatus Canceled = new(8, "Canceled");
    public static readonly OrderStatus Refunded = new(9, "Refunded");
    public static readonly OrderStatus PaymentFailed = new(10, "PaymentFailed");
}
