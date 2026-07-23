using HomeOS.Data;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Calendar;

// Calendar registers itself for search - see Docs/02_Pravila_Programiranja.md, section 1.2.
public class EventSearchProvider : ISearchable
{
    private readonly ApplicationDbContext _context;

    public EventSearchProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResult>> SearchAsync(int householdId, string query)
    {
        var matches = await _context.Events
            .Where(e => e.HouseholdId == householdId && !e.IsDeleted
                && (e.Title.Contains(query) || (e.Location != null && e.Location.Contains(query))))
            .ToListAsync();

        return matches
            .Select(e => new SearchResult("Calendar", e.Title, e.Location, $"/Calendar/Edit/{e.Id}"))
            .ToList();
    }
}
