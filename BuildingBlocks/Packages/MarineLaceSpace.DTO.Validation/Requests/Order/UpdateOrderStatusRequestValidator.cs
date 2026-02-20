using FluentValidation;
using MarineLaceSpace.DTO.Requests.Order;

namespace MarineLaceSpace.DTO.Validation.Requests.Order;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.StatusId)
            .InclusiveBetween(1, 10).WithMessage("Status ID must be between 1 and 10.");
    }
}
