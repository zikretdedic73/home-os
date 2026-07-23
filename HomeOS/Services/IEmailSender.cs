namespace HomeOS.Services;

// Core service - Resend integration, used by Reminders (due notifications)
// and later Finance (bill warnings). See Docs/02_Pravila_Programiranja.md,
// section 1.3 - do not write a second email integration per module.
public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);
}
