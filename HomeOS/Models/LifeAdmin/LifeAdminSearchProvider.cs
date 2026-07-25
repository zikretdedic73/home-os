using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.LifeAdmin;

// Life Admin registers itself for universal search - documents by name/category
// and contacts by name/role. Honors visibility like every other provider.
public class LifeAdminSearchProvider : ISearchable
{
    private readonly ApplicationDbContext _context;
    public LifeAdminSearchProvider(ApplicationDbContext context) => _context = context;

    public async Task<List<SearchResult>> SearchAsync(int householdId, int memberId, string query)
    {
        var documents = await _context.Documents
            .Where(d => d.HouseholdId == householdId && !d.IsDeleted
                && (d.Name.Contains(query) || (d.Category != null && d.Category.Contains(query))))
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Document)
            .ToListAsync();

        var contacts = await _context.Contacts
            .Where(c => c.HouseholdId == householdId && !c.IsDeleted
                && (c.Name.Contains(query) || (c.Role != null && c.Role.Contains(query))))
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Document)
            .ToListAsync();

        var results = documents
            .Select(d => new SearchResult("LifeAdmin", d.Name, d.Category, $"/LifeAdmin/EditDocument/{d.Id}"))
            .ToList();

        results.AddRange(contacts
            .Select(c => new SearchResult("LifeAdmin", c.Name, c.Role, $"/LifeAdmin/EditContact/{c.Id}")));

        return results;
    }
}
