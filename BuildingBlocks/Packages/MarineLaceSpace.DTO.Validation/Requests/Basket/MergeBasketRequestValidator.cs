using FluentValidation;
using MarineLaceSpace.DTO.Requests.Basket;

namespace MarineLaceSpace.DTO.Validation.Requests.Basket;

public class MergeBasketRequestValidator : AbstractValidator<MergeBasketRequest>
{
    public MergeBasketRequestValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("Session ID is required for basket merge.");
    }
}
