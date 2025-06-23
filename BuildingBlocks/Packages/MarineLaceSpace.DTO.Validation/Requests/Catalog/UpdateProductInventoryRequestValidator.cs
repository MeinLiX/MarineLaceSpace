using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class UpdateProductInventoryRequestValidator : AbstractValidator<UpdateProductInventoryRequest>
{
    public UpdateProductInventoryRequestValidator()
    {
        RuleFor(x => x.InventoryItems)
            .NotEmpty().WithMessage("Inventory items list cannot be empty.");

        RuleForEach(x => x.InventoryItems)
            .SetValidator(new ProductInventoryItemValidator());
    }
}
