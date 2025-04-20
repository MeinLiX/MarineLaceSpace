using MarineLaceSpace.Options.Attributes;

namespace MarineLaceSpace.Options;

[Option("JwtSettings")]
public class JwtSettingsOption
{
    public required string SecretKeyBase64 { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required int AccessTokenExpirationMinutes { get; set; }
    public required int RefreshTokenExpirationDays { get; set; }
}
