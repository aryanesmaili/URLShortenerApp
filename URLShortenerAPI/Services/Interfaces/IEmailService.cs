namespace URLShortenerAPI.Services.Interfaces
{
    internal interface IEmailService
    {
        void SendEmail(string to, string subject, string body);
    }
}
