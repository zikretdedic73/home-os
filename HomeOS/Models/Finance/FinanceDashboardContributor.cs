using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Finance;

// Contributes an "upcoming bills" section to the Today dashboard - unpaid bills
// due within the next week (Docs/01 - Dashboard "predstojeće račune").
public class FinanceDashboardContributor : IDashboardContributor
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<FinanceDashboardContributor> _localizer;

    public FinanceDashboardContributor(ApplicationDbContext context, IStringLocalizer<FinanceDashboardContributor> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public string ModuleKey => "Finance";
    public int SortOrder => 70;

    public async Task<DashboardWidget> BuildAsync(int householdId, int memberId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var weekAhead = today.AddDays(7);

        var bills = await _context.Bills
            .Where(b => b.HouseholdId == householdId && !b.IsDeleted && !b.IsPaid && b.DueDate <= weekAhead)
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Bill)
            .OrderBy(b => b.DueDate)
            .ToListAsync();

        var items = bills.Select(b => new DashboardItem(
            $"{b.Name} — {b.Amount:0.00}",
            $"/Finance/EditBill/{b.Id}",
            b.DueDate < today ? _localizer["OverdueBadge"].Value : b.DueDate.ToString("dd.MM."),
            b.DueDate < today)).ToList();

        return new DashboardWidget("Finance", SortOrder, _localizer["Heading"].Value, _localizer["Empty"].Value, items);
    }
}
