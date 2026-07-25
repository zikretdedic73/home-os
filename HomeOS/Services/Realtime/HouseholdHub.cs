using System.Security.Claims;
using HomeOS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Services.Realtime;

// Real-time channel for a household (Docs/00 - "Sinhronizacija u realnom
// vremenu — izmjene jednog člana odmah su vidljive svima"). Each connection
// joins a group scoped to its household, so a broadcast only reaches that
// household's members. The hub itself carries no business logic - it is just
// the transport; what to refresh is decided on the client.
[Authorize]
public class HouseholdHub : Hub
{
    private readonly ApplicationDbContext _context;

    public HouseholdHub(ApplicationDbContext context)
    {
        _context = context;
    }

    public static string GroupName(int householdId) => $"household-{householdId}";

    public override async Task OnConnectedAsync()
    {
        // Resolve household straight from the connection's principal - in a hub
        // there is no ambient HttpContext, so we can't reuse the request-scoped
        // ICurrentHouseholdService here.
        var identityUserId = Context.UserIdentifier
            ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(identityUserId))
        {
            var householdId = await _context.Members
                .Where(m => m.IdentityUserId == identityUserId)
                .Select(m => (int?)m.HouseholdId)
                .FirstOrDefaultAsync();

            if (householdId.HasValue)
                await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(householdId.Value));
        }

        await base.OnConnectedAsync();
    }
}
