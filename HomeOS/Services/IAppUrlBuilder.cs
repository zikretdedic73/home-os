namespace HomeOS.Services;

// Builds absolute URLs for use outside a view (e.g. links inside e-mails).
// Prefers the current request's scheme/host; falls back to the configured
// App:BaseUrl when there is no active request (e.g. a future background job).
public interface IAppUrlBuilder
{
    // Absolute URL to an MVC action, or null if it cannot be built (no request
    // and no configured base URL).
    string? ActionUrl(string action, string controller, object? routeValues = null);
}
