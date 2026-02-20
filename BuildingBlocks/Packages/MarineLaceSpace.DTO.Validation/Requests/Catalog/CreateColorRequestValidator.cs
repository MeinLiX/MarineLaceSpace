using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class CreateColorRequestValidator : AbstractValidator<CreateColorRequest>
{
    public CreateColorRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Color name is required.")
            .Length(1, 50).WithMessage("Color name must be between 1 and 50 characters.");
    }
}
