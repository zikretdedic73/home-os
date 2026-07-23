using HomeOS.Data;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Tasks;

// Tasks registers itself for search - the Shell/SearchService never needs
// to know this module exists (see Docs/02_Pravila_Programiranja.md, section 1.2).
public class TaskSearchProvider : ISearchable
{
    private readonly ApplicationDbContext _context;

    public TaskSearchProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResult>> SearchAsync(int householdId, string query)
    {
        var matches = await _context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted
                && (t.Title.Contains(query) || (t.Description != null && t.Description.Contains(query))))
            .ToListAsync();

        return matches
            .Select(t => new SearchResult("Tasks", t.Title, t.Description, $"/Tasks/Edit/{t.Id}"))
            .ToList();
    }
}
