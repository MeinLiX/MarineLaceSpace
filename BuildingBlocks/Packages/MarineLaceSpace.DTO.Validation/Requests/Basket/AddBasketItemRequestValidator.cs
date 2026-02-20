using FluentValidation;
using MarineLaceSpace.DTO.Requests.Basket;

namespace MarineLaceSpace.DTO.Validation.Requests.Basket;

public class AddBasketItemRequestValidator : AbstractValidator<AddBasketItemRequest>
{
    public AddBasketItemRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("ProductId is required.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be at least 1.");
    }
}
