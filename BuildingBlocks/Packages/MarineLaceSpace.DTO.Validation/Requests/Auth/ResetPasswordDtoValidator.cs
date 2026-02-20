using FluentValidation;
using MarineLaceSpace.DTO.Requests.Auth;

namespace MarineLaceSpace.DTO.Validation.Requests.Auth;

public class ResetPasswordDtoValidator : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(dto => dto.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(dto => dto.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");
    }
}
