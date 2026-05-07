namespace URLShortener.Application.Common.Interfaces.Infrastructure
{
    public interface IEmailService
    {
        Task SendEmail(string to, string subject, string body);
    }
}
