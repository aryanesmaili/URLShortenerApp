namespace URLShortener.Application.Utility.Exceptions;

public class NotFoundException : Exception
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
