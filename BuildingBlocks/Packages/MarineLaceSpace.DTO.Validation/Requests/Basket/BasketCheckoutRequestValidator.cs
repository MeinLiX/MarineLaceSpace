using FluentValidation;
using MarineLaceSpace.DTO.Requests.Basket;
using MarineLaceSpace.DTO.Validation.Common;

namespace MarineLaceSpace.DTO.Validation.Requests.Basket;

public class BasketCheckoutRequestValidator : AbstractValidator<BasketCheckoutRequest>
{
    public BasketCheckoutRequestValidator()
    {
        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.")
            .SetValidator(new ShippingAddressDtoValidator());
    }
}
