using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Database.Payment;

public class RefundRecord
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required]
    public string PaymentId { get; set; } = string.Empty;

    [ForeignKey("PaymentId")]
    public virtual PaymentRecord Payment { get; set; } = null!;

    public decimal Amount { get; set; }

    [StringLength(500)]
    public string? Reason { get; set; }

    public int StatusId { get; set; } = 1; // Pending

    [StringLength(100)]
    public string? ProviderRefundId { get; set; }

    public string? InitiatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
}
