using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Reminders;

// Reminders registers itself for search - see Docs/02_Pravila_Programiranja.md, section 1.2.
public class ReminderSearchProvider : ISearchable
{
    private readonly ApplicationDbContext _context;

    public ReminderSearchProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResult>> SearchAsync(int householdId, int memberId, string query)
    {
        var matches = await _context.Reminders
            .Where(r => r.HouseholdId == householdId && !r.IsDeleted && r.Title.Contains(query))
            .VisibleTo(memberId)
            .ToListAsync();

        return matches
            .Select(r => new SearchResult("Reminders", r.Title, null, $"/Reminders/Edit/{r.Id}"))
            .ToList();
    }
}
