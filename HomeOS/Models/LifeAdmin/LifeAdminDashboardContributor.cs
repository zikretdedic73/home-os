using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.LifeAdmin;

// Contributes an "expiring documents" section to the Today dashboard - documents
// whose expiry is within the next 30 days (Docs/01 - evidencija koja ističe).
public class LifeAdminDashboardContributor : IDashboardContributor
{
    private readonly ApplicationDbContext _context;
    private readonly IStringLocalizer<LifeAdminDashboardContributor> _localizer;

    public LifeAdminDashboardContributor(ApplicationDbContext context, IStringLocalizer<LifeAdminDashboardContributor> localizer)
    {
        _context = context;
        _localizer = localizer;
    }

    public string ModuleKey => "LifeAdmin";
    public int SortOrder => 80;

    public async Task<DashboardWidget> BuildAsync(int householdId, int memberId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var soon = today.AddDays(30);

        var docs = await _context.Documents
            .Where(d => d.HouseholdId == householdId && !d.IsDeleted
                && d.ExpiryDate != null && d.ExpiryDate <= soon)
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Document)
            .OrderBy(d => d.ExpiryDate)
            .ToListAsync();

        var items = docs.Select(d => new DashboardItem(
            d.Name,
            $"/LifeAdmin/EditDocument/{d.Id}",
            d.ExpiryDate!.Value < today ? _localizer["ExpiredBadge"].Value : d.ExpiryDate.Value.ToString("dd.MM."),
            d.ExpiryDate!.Value < today)).ToList();

        return new DashboardWidget("LifeAdmin", SortOrder, _localizer["Heading"].Value, _localizer["Empty"].Value, items);
    }
}
