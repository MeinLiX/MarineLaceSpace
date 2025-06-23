using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class UpdateShopRequestValidator : AbstractValidator<UpdateShopRequest>
{
    public UpdateShopRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shop name is required.")
            .Length(3, 100).WithMessage("Shop name must be between 3 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");
    }
}