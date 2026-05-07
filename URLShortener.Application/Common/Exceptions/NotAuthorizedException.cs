namespace URLShortener.Application.Common.Exceptions;

public sealed class NotAuthorizedException : Exception
{
    public NotAuthorizedException()
    {

    }
    public NotAuthorizedException(string Message) : base(Message)
    {

    }
    public NotAuthorizedException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
