namespace MarineLaceSpace.Enumerations;

public class PaymentProvider(int id, string name) : Enumeration(id, name)
{
    public static readonly PaymentProvider Stripe = new(1, "Stripe");
    public static readonly PaymentProvider LiqPay = new(2, "LiqPay");
    public static readonly PaymentProvider PayPal = new(3, "PayPal");
}
