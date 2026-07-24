using HomeOS.Data;
using HomeOS.Models.Modules;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Services;

public class MemberAccessService : IMemberAccessService
{
    private readonly ApplicationDbContext _context;

    public MemberAccessService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanAccessAsync(int householdId, int memberId, string moduleKey)
    {
        var row = await _context.MemberModuleAccesses
            .FirstOrDefaultAsync(a => a.HouseholdId == householdId
                && a.MemberId == memberId && a.ModuleKey == moduleKey);

        return row?.CanAccess ?? true; // no row = allowed by default
    }

    public async Task<HashSet<string>> GetRestrictedKeysAsync(int householdId, int memberId)
    {
        var keys = await _context.MemberModuleAccesses
            .Where(a => a.HouseholdId == householdId && a.MemberId == memberId && !a.CanAccess)
            .Select(a => a.ModuleKey)
            .ToListAsync();

        return keys.ToHashSet();
    }

    public async Task SetAccessAsync(int householdId, int memberId, string moduleKey, bool canAccess)
    {
        var row = await _context.MemberModuleAccesses
            .FirstOrDefaultAsync(a => a.HouseholdId == householdId
                && a.MemberId == memberId && a.ModuleKey == moduleKey);

        if (row == null)
        {
            row = new MemberModuleAccess
            {
                HouseholdId = householdId,
                MemberId = memberId,
                ModuleKey = moduleKey,
                CanAccess = canAccess
            };
            _context.MemberModuleAccesses.Add(row);
        }
        else
        {
            row.CanAccess = canAccess;
        }

        await _context.SaveChangesAsync();
    }
}
