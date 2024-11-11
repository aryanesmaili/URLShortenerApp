namespace URLShortenerAPI.Services.Interfaces.UserRelated
{
    public interface IEmailService
    {
        Task SendEmail(string to, string subject, string body);
    }
}
