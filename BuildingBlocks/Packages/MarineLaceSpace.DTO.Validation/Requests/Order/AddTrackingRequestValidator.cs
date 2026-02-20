using FluentValidation;
using MarineLaceSpace.DTO.Requests.Order;

namespace MarineLaceSpace.DTO.Validation.Requests.Order;

public class AddTrackingRequestValidator : AbstractValidator<AddTrackingRequest>
{
    public AddTrackingRequestValidator()
    {
        RuleFor(x => x.TrackingNumber)
            .NotEmpty().WithMessage("Tracking number is required.")
            .MaximumLength(100).WithMessage("Tracking number cannot exceed 100 characters.");
    }
}
