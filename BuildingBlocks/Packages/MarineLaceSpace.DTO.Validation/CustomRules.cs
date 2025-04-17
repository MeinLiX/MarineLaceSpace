using System.Text.RegularExpressions;

namespace MarineLaceSpace.DTO.Validation;

internal static partial class CustomRules
{
    public static bool BeValidPostalCode(string postalCode)
    {
        if (string.IsNullOrEmpty(postalCode))
            return false;

        return postalCode.Length >= 3 && postalCode.Length <= 10;
    }


    [GeneratedRegex(@"^\+?(\d{1,2})?[ .-]?\(?(\d{3})\)?[ .-]?(\d{3})[ .-]?(\d{4})$")]
    private static partial Regex PhoneNumberRegex();

    public static bool BeValidPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return false;

        return PhoneNumberRegex().IsMatch(phoneNumber);
    }
}
