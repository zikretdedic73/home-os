using HomeOS.Data;
using HomeOS.Models.Modules;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Services;

public class PermissionService : IPermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasPermissionAsync(int householdId, string moduleKey, string permission)
    {
        var state = await _context.ModulePermissionStates
            .FirstOrDefaultAsync(p => p.HouseholdId == householdId
                && p.ModuleKey == moduleKey && p.Permission == permission);

        // No row = granted by default (built-in trusted module).
        return state?.IsGranted ?? true;
    }

    public async Task SetPermissionAsync(int householdId, string moduleKey, string permission, bool granted)
    {
        var state = await _context.ModulePermissionStates
            .FirstOrDefaultAsync(p => p.HouseholdId == householdId
                && p.ModuleKey == moduleKey && p.Permission == permission);

        if (state == null)
        {
            state = new ModulePermissionState
            {
                HouseholdId = householdId,
                ModuleKey = moduleKey,
                Permission = permission,
                IsGranted = granted
            };
            _context.ModulePermissionStates.Add(state);
        }
        else
        {
            state.IsGranted = granted;
        }

        await _context.SaveChangesAsync();
    }
}
