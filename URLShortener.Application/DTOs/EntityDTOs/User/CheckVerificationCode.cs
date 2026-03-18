namespace URLShortener.Application.DTOs.EntityDTOs.User;

public sealed class CheckVerificationCode
{
    public int? ID { get; set; }

    private string _identifier = string.Empty;
    public string Identifier { get => _identifier; set => _identifier = value.Trim(); }

    private string _code = string.Empty;
    public string Code { get => _code; set => _code = value.Trim(); }
}

