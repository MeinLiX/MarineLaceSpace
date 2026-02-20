using FluentValidation;
using MarineLaceSpace.DTO.Requests.Auth;

namespace MarineLaceSpace.DTO.Validation.Requests.Auth;

public class UpdateUserProfileDtoValidator : AbstractValidator<UpdateUserProfileDto>
{
    public UpdateUserProfileDtoValidator()
    {
        RuleFor(dto => dto.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.")
            .When(dto => dto.FirstName != null);

        RuleFor(dto => dto.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.")
            .When(dto => dto.LastName != null);

        RuleFor(dto => dto.PhoneNumber)
            .Matches(@"^\+?[\d\s\-\(\)]+$").WithMessage("Invalid phone number format.")
            .When(dto => dto.PhoneNumber != null);
    }
}
