namespace URLShortener.Application.Common.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException()
    {

    }
    public NotFoundException(string message) : base(message)
    {

    }
    public NotFoundException(string entityName, string propertyName, object? value) : base($"{entityName} with {propertyName} '{value}' was not found.")
    {
    }
}
