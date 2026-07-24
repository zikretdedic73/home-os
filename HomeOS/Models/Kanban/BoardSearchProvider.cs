using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Kanban;

// Kanban registers itself for search (boards by name) - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class BoardSearchProvider : ISearchable
{
    private readonly ApplicationDbContext _context;

    public BoardSearchProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResult>> SearchAsync(int householdId, int memberId, string query)
    {
        var matches = await _context.Boards
            .Where(b => b.HouseholdId == householdId && !b.IsDeleted && b.Name.Contains(query))
            .VisibleTo(memberId)
            .ToListAsync();

        return matches
            .Select(b => new SearchResult("Kanban", b.Name, null, $"/Kanban/Board/{b.Id}"))
            .ToList();
    }
}
