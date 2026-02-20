using FluentValidation;
using MarineLaceSpace.DTO.Requests.Payment;

namespace MarineLaceSpace.DTO.Validation.Requests.Payment;

public class RefundPaymentRequestValidator : AbstractValidator<RefundPaymentRequest>
{
    public RefundPaymentRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).When(x => x.Amount.HasValue)
            .WithMessage("Refund amount must be greater than 0.");

        RuleFor(x => x.Reason)
            .MaximumLength(500).When(x => x.Reason != null)
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
