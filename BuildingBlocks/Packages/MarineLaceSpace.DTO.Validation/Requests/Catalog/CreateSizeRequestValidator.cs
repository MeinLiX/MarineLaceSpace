using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class CreateSizeRequestValidator : AbstractValidator<CreateSizeRequest>
{
    public CreateSizeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Size name is required.")
            .Length(1, 50).WithMessage("Size name must be between 1 and 50 characters.");

        RuleFor(x => x.GenderId)
            .InclusiveBetween(1, 3).WithMessage("GenderId must be 1 (Male), 2 (Female), or 3 (Unisex).");
    }
}
