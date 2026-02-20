using FluentValidation;
using MarineLaceSpace.DTO.Requests.Basket;

namespace MarineLaceSpace.DTO.Validation.Requests.Basket;

public class UpdateBasketItemRequestValidator : AbstractValidator<UpdateBasketItemRequest>
{
    public UpdateBasketItemRequestValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.")
            .When(x => x.Quantity.HasValue);

        RuleFor(x => x.Personalization)
            .MaximumLength(512).WithMessage("Personalization text cannot exceed 512 characters.")
            .When(x => x.Personalization != null);
    }
}
