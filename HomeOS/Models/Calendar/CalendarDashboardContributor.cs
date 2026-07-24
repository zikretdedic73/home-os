using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Calendar;

// Calendar contributes its "today's events" section to the dashboard.
public class CalendarDashboardContributor : IDashboardContributor
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<CalendarDashboardContributor> _localizer;

    public CalendarDashboardContributor(ApplicationDbContext context, IStringLocalizer<CalendarDashboardContributor> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public string ModuleKey => "Calendar";
    public int SortOrder => 40;

    public async Task<DashboardWidget> BuildAsync(int householdId, int memberId)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        var events = await _context.Events
            .Where(e => e.HouseholdId == householdId && !e.IsDeleted
                && e.StartsAtUtc < tomorrowUtc && e.EndsAtUtc >= todayUtc)
            .VisibleTo(memberId)
            .OrderBy(e => e.StartsAtUtc)
            .ToListAsync();

        var items = events.Select(e => new DashboardItem(
            $"{e.Title} — {e.StartsAtUtc:HH:mm}",
            $"/Calendar/Edit/{e.Id}",
            null,
            false)).ToList();

        return new DashboardWidget("Calendar", SortOrder, _localizer["Heading"].Value, _localizer["Empty"].Value, items);
    }
}
