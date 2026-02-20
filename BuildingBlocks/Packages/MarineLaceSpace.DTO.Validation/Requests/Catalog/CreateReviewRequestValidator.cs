using FluentValidation;
using MarineLaceSpace.DTO.Requests.Catalog;

namespace MarineLaceSpace.DTO.Validation.Requests.Catalog;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(0, 5).WithMessage("Rating must be between 0 and 5.");

        RuleFor(x => x.Comment)
            .NotEmpty().WithMessage("Comment is required.")
            .MaximumLength(1000).WithMessage("Comment cannot exceed 1000 characters.");
    }
}
