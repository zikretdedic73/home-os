using HomeOS.Data;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Notes;

// Notes contributes a "today's journal" quick access to the dashboard
// (Docs/01_Roadmap.md, section 3.2).
public class NotesDashboardContributor : IDashboardContributor
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<NotesDashboardContributor> _localizer;

    public NotesDashboardContributor(ApplicationDbContext context, IStringLocalizer<NotesDashboardContributor> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public string ModuleKey => "Notes";
    public int SortOrder => 60;

    public async Task<DashboardWidget> BuildAsync(int householdId, int memberId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasEntry = await _context.Notes.AnyAsync(n =>
            n.HouseholdId == householdId && n.OwnerId == memberId && !n.IsDeleted
            && n.IsJournalEntry && n.JournalDate == today);

        var label = hasEntry ? _localizer["OpenTodayJournal"].Value : _localizer["StartTodayJournal"].Value;

        var items = new List<DashboardItem>
        {
            new(label, "/Notes/Journal", null, false)
        };

        return new DashboardWidget("Notes", SortOrder, _localizer["Heading"].Value, _localizer["Empty"].Value, items);
    }
}
