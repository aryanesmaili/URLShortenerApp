using System.Net;
using System.Net.Mail;
using URLShortener.Application.DTOs.Settings;
using URLShortener.Application.Interfaces.Infrastructure.External;

namespace URLShortener.Infrastructure.Services.User;

public sealed class EmailService : IEmailService
{
    private readonly SMTPSettings _smtpSettings;
    private readonly SmtpClient _smtpClient;
    private readonly MailAddress _mailAddress;
    public EmailService(SMTPSettings smtpSettings)
    {
        _smtpSettings = smtpSettings;
        _smtpClient = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port)
        {
            Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
            EnableSsl = true
        };
        _mailAddress = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName);
    }

    /// <summary>
    /// Sends an Email using the SMTP settings provided in Config file.
    /// </summary>
    /// <param name="to">receiver of the email.</param>
    /// <param name="subject">Subject of the email.</param>
    /// <param name="body">Body of the email.</param>
    public async Task SendEmail(string to, string subject, string body)
    {
        MailMessage mailMessage = new()
        {
            From = _mailAddress,
            Subject = subject,
            Body = body,
            IsBodyHtml = false,
        };
        mailMessage.To.Add(to);

        await _smtpClient.SendMailAsync(mailMessage);
    }
}
