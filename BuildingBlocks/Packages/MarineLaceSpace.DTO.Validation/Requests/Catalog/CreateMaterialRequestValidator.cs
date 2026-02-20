using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class CreateMaterialRequestValidator : AbstractValidator<CreateMaterialRequest>
{
    public CreateMaterialRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Material name is required.")
            .Length(1, 100).WithMessage("Material name must be between 1 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
