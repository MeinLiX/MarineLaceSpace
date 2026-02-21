using MarineLaceSpace.Options.Attributes;

namespace MarineLaceSpace.Options;

[Option("AdminSeed")]
public class AdminSeedOption
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string FirstName { get; set; } = "Admin";
    public string LastName { get; set; } = "MarineLaceSpace";
}
