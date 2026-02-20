using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required.")
            .Length(2, 100).WithMessage("Category name must be between 2 and 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}
