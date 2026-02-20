using FluentValidation;
using MarineLaceSpace.DTO.Requests.Auth;

namespace MarineLaceSpace.DTO.Validation.Requests.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(dto => dto.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(dto => dto.Password)
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.")
            .When(dto => !dto.IsAnonumous);

        RuleFor(dto => dto.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.")
            .When(dto => dto.FirstName != null);

        RuleFor(dto => dto.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.")
            .When(dto => dto.LastName != null);
    }
}
