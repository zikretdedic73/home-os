namespace HomeOS.Services;

// Per-member module access (RBAC). The owner restricts which modules a member
// may open; everything is allowed by default (Docs/01_Roadmap.md, section 3.4).
public interface IMemberAccessService
{
    Task<bool> CanAccessAsync(int householdId, int memberId, string moduleKey);

    // Module keys the member is explicitly restricted from (CanAccess=false).
    Task<HashSet<string>> GetRestrictedKeysAsync(int householdId, int memberId);

    Task SetAccessAsync(int householdId, int memberId, string moduleKey, bool canAccess);
}
