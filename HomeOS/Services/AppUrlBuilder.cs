using Microsoft.AspNetCore.Routing;

namespace HomeOS.Services;

public class AppUrlBuilder : IAppUrlBuilder
{
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public AppUrlBuilder(LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public string? ActionUrl(string action, string controller, object? routeValues = null)
    {
        var values = routeValues == null ? null : new RouteValueDictionary(routeValues);

        // In-request path: reuse the live scheme/host so links are correct in
        // any environment (localhost, staging, prod) without configuration.
        var http = _httpContextAccessor.HttpContext;
        if (http != null)
            return _linkGenerator.GetUriByAction(http, action, controller, values);

        // Out-of-request fallback: build from the configured absolute base URL.
        var baseUrl = _configuration["App:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl) || !Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
            return null;

        return _linkGenerator.GetUriByAction(action, controller, values,
            scheme: uri.Scheme, host: new HostString(uri.Host, uri.IsDefaultPort ? -1 : uri.Port));
    }
}
