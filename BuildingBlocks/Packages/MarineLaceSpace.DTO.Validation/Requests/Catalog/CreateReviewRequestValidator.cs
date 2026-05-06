using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 5).WithMessage("Rating must be between 0 and 5.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Text) || !string.IsNullOrEmpty(x.Comment))
            .WithMessage("Review text is required.")
            .OverridePropertyName("Text");
    }
}
