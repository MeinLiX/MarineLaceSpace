using System.ComponentModel.DataAnnotations;

namespace MarineLaceSpace.Models.Database.Payment;

public class PaymentRecord
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string OrderId { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    [StringLength(10)]
    public string Currency { get; set; } = "UAH";

    public int ProviderId { get; set; } = 1;

    public string? ProviderPaymentId { get; set; }

    public int StatusId { get; set; } = 1;

    public string? BuyerEmail { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
