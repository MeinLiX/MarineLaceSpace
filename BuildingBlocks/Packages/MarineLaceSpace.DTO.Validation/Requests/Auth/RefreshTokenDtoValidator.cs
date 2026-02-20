using FluentValidation;
using MarineLaceSpace.DTO.Requests.Auth;

namespace MarineLaceSpace.DTO.Validation.Requests.Auth;

public class RefreshTokenDtoValidator : AbstractValidator<RefreshTokenDto>
{
    public RefreshTokenDtoValidator()
    {
        RuleFor(dto => dto.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
