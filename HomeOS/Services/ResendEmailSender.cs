using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;

namespace HomeOS.Services;

// Calls the Resend API directly over HTTP (see https://resend.com/docs/api-reference/emails/send-email) -
// no extra NuGet dependency for a single POST call. Account creation and
// domain verification are done outside the code, per Docs/01_Roadmap.md
// section 2.2 - if no API key is configured yet, sending is skipped rather
// than crashing the app, so the rest of the module still works in dev.
public class ResendEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly ResendOptions _options;
    private readonly ILogger<ResendEmailSender> _logger;

    public ResendEmailSender(HttpClient httpClient, IOptions<ResendOptions> options, ILogger<ResendEmailSender> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogInformation("Resend API key is not configured - skipping email to {ToEmail}.", toEmail);
            return;
        }

        _httpClient.BaseAddress ??= new Uri("https://api.resend.com/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var payload = new
        {
            from = $"{_options.FromName} <{_options.FromEmail}>",
            to = new[] { toEmail },
            subject,
            html = htmlBody
        };

        var response = await _httpClient.PostAsJsonAsync("emails", payload);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Resend email to {ToEmail} failed with status {StatusCode}: {Body}", toEmail, response.StatusCode, body);
        }
    }
}
