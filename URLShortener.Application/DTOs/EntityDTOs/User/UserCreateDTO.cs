namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record UserCreateDTO
{
    private string name = string.Empty;

    public required string Name
    {
        get => name;
        init => name = value.Trim();
    }

    private string email = string.Empty;

    public required string Email
    {
        get => email;
        init => email = value.Trim();
    }

    private string username = string.Empty;

    public required string Username
    {
        get => username;
        init => username = value.Trim();
    }

    private string password = string.Empty;

    public required string Password
    {
        get => password;
        init => password = value.Trim();
    }

    private string confirmPassword = string.Empty;

    public required string ConfirmPassword
    {
        get => confirmPassword;
        init => confirmPassword = value.Trim();
    }
}

