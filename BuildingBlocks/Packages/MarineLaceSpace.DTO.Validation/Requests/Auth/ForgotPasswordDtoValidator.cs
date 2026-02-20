using FluentValidation;
using MarineLaceSpace.DTO.Requests.Auth;

namespace MarineLaceSpace.DTO.Validation.Requests.Auth;

public class ForgotPasswordDtoValidator : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}
