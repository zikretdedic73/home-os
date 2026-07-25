using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace HomeOS.Services;

// Sends mail through Gmail's SMTP server using MailKit. Implements the same
// IEmailSender contract as before, so no caller changes when swapping providers
// (Docs/02 - one shared email integration). If no App Password is configured
// yet, sending is skipped (returns false) rather than crashing, so the rest of
// the app still works in dev - same graceful behavior as the old sender.
public class GmailEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<GmailEmailSender> _logger;

    public GmailEmailSender(IOptions<SmtpOptions> options, ILogger<GmailEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_options.AppPassword) || string.IsNullOrWhiteSpace(_options.FromEmail))
        {
            _logger.LogInformation("SMTP is not configured (no App Password / from address) - skipping email to {ToEmail}.", toEmail);
            return false;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            // Gmail on 587 uses STARTTLS; MailKit picks the matching TLS mode.
            await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls);
            // The username is the Gmail address; the password is the App Password.
            await client.AuthenticateAsync(_options.FromEmail, _options.AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            return true;
        }
        catch (Exception ex)
        {
            // A failed send must not break the caller (e.g. task/reminder still
            // saved) - log and report failure so callers can decide to retry.
            _logger.LogWarning(ex, "SMTP email to {ToEmail} failed.", toEmail);
            return false;
        }
    }
}
