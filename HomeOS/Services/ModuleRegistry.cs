using HomeOS.Data;
using HomeOS.Models.Modules;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Services;

public class ModuleRegistry : IModuleRegistry
{
    private readonly IEnumerable<IModuleDescriptor> _descriptors;
    private readonly ApplicationDbContext _context;

    public ModuleRegistry(IEnumerable<IModuleDescriptor> descriptors, ApplicationDbContext context)
    {
        _descriptors = descriptors;
        _context = context;
    }

    public async Task<IReadOnlyList<ModuleInfo>> GetAllAsync(int householdId)
    {
        var disabledKeys = await GetDisabledKeysAsync(householdId);

        return _descriptors
            .OrderBy(d => d.SortOrder)
            .Select(d => new ModuleInfo(d, !disabledKeys.Contains(d.Key)))
            .ToList();
    }

    public async Task<IReadOnlyList<IModuleDescriptor>> GetEnabledAsync(int householdId)
    {
        var disabledKeys = await GetDisabledKeysAsync(householdId);

        return _descriptors
            .Where(d => !disabledKeys.Contains(d.Key))
            .OrderBy(d => d.SortOrder)
            .ToList();
    }

    public async Task SetEnabledAsync(int householdId, string moduleKey, bool enabled)
    {
        // Ignore unknown keys so a stale form post can't create junk rows.
        if (_descriptors.All(d => d.Key != moduleKey))
            return;

        var state = await _context.ModuleStates
            .FirstOrDefaultAsync(s => s.HouseholdId == householdId && s.ModuleKey == moduleKey);

        if (state == null)
        {
            state = new ModuleState { HouseholdId = householdId, ModuleKey = moduleKey, IsEnabled = enabled };
            _context.ModuleStates.Add(state);
        }
        else
        {
            state.IsEnabled = enabled;
        }

        await _context.SaveChangesAsync();
    }

    private async Task<HashSet<string>> GetDisabledKeysAsync(int householdId)
    {
        var keys = await _context.ModuleStates
            .Where(s => s.HouseholdId == householdId && !s.IsEnabled)
            .Select(s => s.ModuleKey)
            .ToListAsync();

        return keys.ToHashSet();
    }
}
