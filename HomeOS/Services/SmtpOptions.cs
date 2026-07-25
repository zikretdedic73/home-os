namespace HomeOS.Services;

// SMTP settings for the Gmail sender. Host/Port/From live in appsettings; the
// AppPassword is a secret and must come from user-secrets / environment, never
// from a committed file (Docs/02 - secrets out of source control). With a Gmail
// App Password (account has 2-Step Verification on) mail can be sent to any
// recipient - unlike the earlier Resend sandbox restriction.
public class SmtpOptions
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;           // STARTTLS
    public string FromEmail { get; set; } = string.Empty;   // the Gmail address
    public string FromName { get; set; } = "Home OS";
    public string AppPassword { get; set; } = string.Empty; // 16-char Google App Password
}
