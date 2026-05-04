using System.Text.Json.Serialization;
using URLShortener.Common.HelperFunctions;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record ChangePasswordRequest
{
    private string identifier = string.Empty;
    public string Identifier
    {
        get => identifier;
        init => identifier = value.Trim();
    }

    [JsonIgnore]
    public bool IdentifierIsEmail => Identifier.IsEmail();
}

