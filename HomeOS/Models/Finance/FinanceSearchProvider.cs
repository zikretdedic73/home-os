using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Models.Finance;

// Finance registers itself for universal search - transactions by description
// and bills by name. Honors visibility like every other provider.
public class FinanceSearchProvider : ISearchable
{
    private readonly ApplicationDbContext _context;
    public FinanceSearchProvider(ApplicationDbContext context) => _context = context;

    public async Task<List<SearchResult>> SearchAsync(int householdId, int memberId, string query)
    {
        var transactions = await _context.Transactions
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted && t.Description.Contains(query))
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Transaction)
            .ToListAsync();

        var bills = await _context.Bills
            .Where(b => b.HouseholdId == householdId && !b.IsDeleted && b.Name.Contains(query))
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Bill)
            .ToListAsync();

        var results = transactions
            .Select(t => new SearchResult("Finance", t.Description, t.Amount.ToString("0.00"), $"/Finance/EditTransaction/{t.Id}"))
            .ToList();

        results.AddRange(bills
            .Select(b => new SearchResult("Finance", b.Name, b.Amount.ToString("0.00"), $"/Finance/EditBill/{b.Id}")));

        return results;
    }
}
