namespace URLShortener.Application.Utility.Exceptions;

public sealed class InsufficientBalanceException : Exception
{
    public InsufficientBalanceException()
    {
    }

    public InsufficientBalanceException(string? message) : base(message)
    {
    }

    public InsufficientBalanceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
