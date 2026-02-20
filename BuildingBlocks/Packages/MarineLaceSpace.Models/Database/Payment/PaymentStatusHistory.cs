using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MarineLaceSpace.Models.Database.Payment;

public class PaymentStatusHistory
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string PaymentId { get; set; } = string.Empty;

    [ForeignKey("PaymentId")]
    public virtual PaymentRecord Payment { get; set; } = null!;

    public int OldStatusId { get; set; }
    public int NewStatusId { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
