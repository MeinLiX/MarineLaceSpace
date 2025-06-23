using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class CreateShopRequestValidator : AbstractValidator<CreateShopRequest>
{
    public CreateShopRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shop name is required.")
            .Length(3, 100).WithMessage("Shop name must be between 3 and 100 characters.");

        RuleFor(x => x.UrlSlug)
            .NotEmpty().WithMessage("URL slug is required.")
            .Length(3, 100).WithMessage("URL slug must be between 3 and 100 characters.")
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("URL slug can only contain lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");
    }
}