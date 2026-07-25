using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Notes;

// Notes registers itself for search (title + content) - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class NoteSearchProvider : ISearchable
{
    private readonly ApplicationDbContext _context;

    public NoteSearchProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResult>> SearchAsync(int householdId, int memberId, string query)
    {
        var matches = await _context.Notes
            .Where(n => n.HouseholdId == householdId && !n.IsDeleted
                && ((n.Title != null && n.Title.Contains(query)) || n.Content.Contains(query)))
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Note)
            .ToListAsync();

        return matches
            .Select(n => new SearchResult(
                "Notes",
                string.IsNullOrEmpty(n.Title) ? (n.JournalDate?.ToString("dd.MM.yyyy") ?? "Note") : n.Title,
                n.Content.Length > 80 ? n.Content[..80] : n.Content,
                $"/Notes/Edit/{n.Id}"))
            .ToList();
    }
}
