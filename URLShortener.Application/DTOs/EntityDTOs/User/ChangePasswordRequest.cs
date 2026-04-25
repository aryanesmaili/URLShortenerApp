using System.Text.Json.Serialization;
using URLShortener.Common.HelperFunctions;

namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed record ChangePasswordRequest
{
    public string Identifier { get; init; } = string.Empty;

    [JsonIgnore]
    public bool IdentifierIsEmail => Identifier.IsEmail();
}

