namespace URLShortenerAPI.Data.Interfaces.User
{
    public interface IEmailService
    {
        Task SendEmail(string to, string subject, string body);
    }
}
