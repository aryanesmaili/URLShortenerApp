namespace URLShortener.Application.Interfaces.Infrastructure.External
{
    public interface IEmailService
    {
        Task SendEmail(string to, string subject, string body);
    }
}
