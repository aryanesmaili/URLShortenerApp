namespace URLShortener.Application.Utility.Exceptions;

public sealed class RefreshTokenExpiredException : Exception
{
    public RefreshTokenExpiredException()
    {

    }
    public RefreshTokenExpiredException(string message) : base(message)
    {

    }
    public RefreshTokenExpiredException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
