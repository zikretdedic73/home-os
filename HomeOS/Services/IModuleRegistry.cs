namespace HomeOS.Services;

// One module plus whether it is currently enabled for the household.
public record ModuleInfo(IModuleDescriptor Descriptor, bool IsEnabled);

// Aggregates every registered IModuleDescriptor with its persisted
// enable/disable state. Navigation, search and the module manager all read
// from here, so none of them needs to know the concrete module set.
public interface IModuleRegistry
{
    // All modules (enabled and disabled), sorted - for the module manager.
    Task<IReadOnlyList<ModuleInfo>> GetAllAsync(int householdId);

    // Only enabled modules, sorted - for navigation and search.
    Task<IReadOnlyList<IModuleDescriptor>> GetEnabledAsync(int householdId);

    Task SetEnabledAsync(int householdId, string moduleKey, bool enabled);
}
