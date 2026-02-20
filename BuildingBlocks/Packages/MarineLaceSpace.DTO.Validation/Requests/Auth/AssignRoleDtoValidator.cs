using FluentValidation;
using MarineLaceSpace.DTO.Requests.Auth;

namespace MarineLaceSpace.DTO.Validation.Requests.Auth;

public class AssignRoleDtoValidator : AbstractValidator<AssignRoleDto>
{
    public AssignRoleDtoValidator()
    {
        RuleFor(dto => dto.Roles)
            .NotEmpty().WithMessage("At least one role must be specified.");

        RuleForEach(dto => dto.Roles)
            .NotEmpty().WithMessage("Role name cannot be empty.");
    }
}
