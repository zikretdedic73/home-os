using HomeOS.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;

namespace HomeOS.Services.Realtime;

// Turns every successful data-changing request into a real-time signal, in one
// place, so individual controllers don't each have to publish a "changed"
// event. After any authenticated POST that completed without throwing, it tells
// the caller's household that <module> changed; connected members' pages decide
// whether to refresh. This keeps the whole app "live" with a single seam.
public class HouseholdBroadcastFilter : IAsyncActionFilter
{
    private readonly IHubContext<HouseholdHub> _hub;
    private readonly ICurrentHouseholdService _household;
    private readonly ILogger<HouseholdBroadcastFilter> _logger;

    public HouseholdBroadcastFilter(
        IHubContext<HouseholdHub> hub,
        ICurrentHouseholdService household,
        ILogger<HouseholdBroadcastFilter> logger)
    {
        _hub = hub;
        _household = household;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executed = await next();

        if (!HttpMethods.IsPost(context.HttpContext.Request.Method)) return;
        if (context.HttpContext.User?.Identity?.IsAuthenticated != true) return;
        if (executed.Exception != null && !executed.ExceptionHandled) return;

        var controller = (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerName;
        if (string.IsNullOrEmpty(controller)) return;

        try
        {
            var householdId = await _household.GetCurrentHouseholdIdAsync();
            await _hub.Clients.Group(HouseholdHub.GroupName(householdId))
                .SendAsync("dataChanged", new { module = controller });
        }
        catch (Exception ex)
        {
            // A broadcast failure must never affect the user's actual response.
            _logger.LogDebug(ex, "Real-time broadcast skipped after {Controller} POST.", controller);
        }
    }
}
