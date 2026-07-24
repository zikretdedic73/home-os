using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Reminders;

// Reminders contributes its "active reminders" section to the dashboard.
public class RemindersDashboardContributor : IDashboardContributor
{
    private readonly IReminderNotificationService _reminders;
    private readonly IStringLocalizer<RemindersDashboardContributor> _localizer;

    public RemindersDashboardContributor(IReminderNotificationService reminders, IStringLocalizer<RemindersDashboardContributor> localizer)
    {
        _reminders = reminders;
        _localizer = localizer;
    }

    public string ModuleKey => "Reminders";
    public int SortOrder => 50;

    public async Task<DashboardWidget> BuildAsync(int householdId, int memberId)
    {
        var active = await _reminders.GetActiveRemindersForMemberAsync(householdId, memberId);

        var items = active.Select(r => new DashboardItem(
            r.Title,
            $"/Reminders/Edit/{r.Id}",
            null,
            false)).ToList();

        return new DashboardWidget("Reminders", SortOrder, _localizer["Heading"].Value, _localizer["Empty"].Value, items);
    }
}
