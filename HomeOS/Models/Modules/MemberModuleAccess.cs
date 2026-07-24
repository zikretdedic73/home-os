namespace HomeOS.Models.Modules;

// Per-member access to a module (RBAC) - the owner decides which modules each
// member may open/see (Docs/01_Roadmap.md, section 3.4; distinct from
// ModulePermissionState which is module-to-module data access). Absence of a
// row means the member may access the module (default allow); a row with
// CanAccess=false means the owner restricted it for that member.
public class MemberModuleAccess
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public int MemberId { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public bool CanAccess { get; set; } = true;
}
