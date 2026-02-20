using FluentValidation;
using MarineLaceSpace.DTO.Requests.Auth;

namespace MarineLaceSpace.DTO.Validation.Requests.Auth;

public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(dto => dto.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(dto => dto.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("New password must be at least 6 characters.");

        RuleFor(dto => dto.NewPassword)
            .NotEqual(dto => dto.CurrentPassword)
            .WithMessage("New password must be different from current password.");
    }
}
