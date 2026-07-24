using HomeOS.Data;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Tasks;

// Tasks contributes its "due & overdue" section to the dashboard - see
// Docs/02_Pravila_Programiranja.md, section 1.6.
public class TasksDashboardContributor : IDashboardContributor
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<TasksDashboardContributor> _localizer;

    public TasksDashboardContributor(ApplicationDbContext context, IStringLocalizer<TasksDashboardContributor> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public string ModuleKey => "Tasks";
    public int SortOrder => 20;

    public async Task<DashboardWidget> BuildAsync(int householdId, int memberId)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        var tasks = await _context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted
                && t.Status != TaskState.Done
                && t.DueDate != null && t.DueDate < tomorrowUtc)
            .OrderBy(t => t.DueDate)
            .ToListAsync();

        var items = tasks.Select(t => new DashboardItem(
            t.Title,
            $"/Tasks/Edit/{t.Id}",
            t.DueDate!.Value.Date < todayUtc ? _localizer["OverdueBadge"].Value : null,
            t.DueDate!.Value.Date < todayUtc)).ToList();

        return new DashboardWidget("Tasks", SortOrder, _localizer["Heading"].Value, _localizer["Empty"].Value, items);
    }
}
