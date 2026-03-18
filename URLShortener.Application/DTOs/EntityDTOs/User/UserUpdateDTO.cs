namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class UserUpdateDTO
{
    public int ID { get; set; }

    private string? _name;
    public string? Name
    {
        get => _name;
        set => _name = value?.Trim();
    }

    private string? _username;
    public string? Username
    {
        get => _username;
        set => _username = value?.Trim();
    }
}

