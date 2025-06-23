using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class ProductInventoryItemValidator : AbstractValidator<ProductInventoryItem>
{
    public ProductInventoryItemValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price for each variation must be greater than 0.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity for each variation must be 0 or greater.");
    }
}