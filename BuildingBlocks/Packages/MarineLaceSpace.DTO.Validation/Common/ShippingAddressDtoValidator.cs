using FluentValidation;
using MarineLaceSpace.DTO.Common;

namespace MarineLaceSpace.DTO.Validation.Common;

public class ShippingAddressDtoValidator : AbstractValidator<ShippingAddressDto>
{
    public ShippingAddressDtoValidator()
    {
        RuleFor(address => address.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters");

        RuleFor(address => address.AddressLine1)
            .NotEmpty().WithMessage("Address line 1 is required")
            .Length(2, 100).WithMessage("Address must be between 2 and 100 characters");

        RuleFor(address => address.AddressLine2)
            .Length(0, 100).WithMessage("Address line 2 should not exceed 100 characters");

        RuleFor(address => address.City)
            .NotEmpty().WithMessage("City is required")
            .Length(2, 50).WithMessage("City name must be between 2 and 50 characters");

        RuleFor(address => address.PostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .Must(CustomRules.BeValidPostalCode).WithMessage("Invalid postal code format");

        RuleFor(address => address.Country)
            .NotEmpty().WithMessage("Country is required")
            .Length(2, 50).WithMessage("Country name must be between 2 and 50 characters");

        RuleFor(address => address.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Must(CustomRules.BeValidPhoneNumber).WithMessage("Invalid phone number format");
    }
}
