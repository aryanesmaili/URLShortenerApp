namespace URLShortener.Common.Responses;

public class BaseResponse
{
    public bool Success { get; init; }
    public required string Message { get; init; }
}
