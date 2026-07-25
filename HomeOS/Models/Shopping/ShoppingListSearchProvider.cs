using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Shopping;

// Shopping Lists registers itself for search (list name or item name) - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class ShoppingListSearchProvider : ISearchable
{
    private readonly ApplicationDbContext _context;

    public ShoppingListSearchProvider(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SearchResult>> SearchAsync(int householdId, int memberId, string query)
    {
        var matches = await _context.ShoppingLists
            .Where(l => l.HouseholdId == householdId && !l.IsDeleted
                && (l.Name.Contains(query) || l.Items.Any(i => i.Name.Contains(query))))
            .VisibleTo(memberId, _context.ItemShares, ShareableType.ShoppingList)
            .ToListAsync();

        return matches
            .Select(l => new SearchResult("ShoppingLists", l.Name, null, $"/ShoppingLists/List/{l.Id}"))
            .ToList();
    }
}
