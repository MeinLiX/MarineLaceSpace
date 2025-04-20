namespace MarineLaceSpace.DTO.Requests.Auth;

public class RegisterDto
{
    public string Email { get; set; }
    public string? Password { get; set; }

    public bool IsAnonumous => string.IsNullOrWhiteSpace(Password);
}
