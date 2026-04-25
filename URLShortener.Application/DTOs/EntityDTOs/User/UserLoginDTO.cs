using System.Text.Json.Serialization;
using URLShortener.Common.HelperFunctions;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record UserLoginDTO
{
    private string _identifier = string.Empty;

    public string Identifier
    {
        get => _identifier;
        init => _identifier = value.Trim();
    }

    private string _password = string.Empty;

    public string Password
    {
        get => _password;
        init => _password = value.Trim();
    }

    [JsonIgnore]
    public bool IsEmailIdentifier => HelperFunctions.IsEmail(Identifier ?? string.Empty);
}

