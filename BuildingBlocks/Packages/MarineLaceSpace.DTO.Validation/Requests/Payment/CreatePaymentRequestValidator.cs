using FluentValidation;
using MarineLaceSpace.DTO.Requests.Payment;

namespace MarineLaceSpace.DTO.Validation.Requests.Payment;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

        RuleFor(x => x.ProviderId)
            .InclusiveBetween(1, 3).WithMessage("Provider ID must be 1 (Stripe), 2 (LiqPay), or 3 (PayPal).");
    }
}
