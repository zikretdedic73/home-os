using HomeOS.Services;

namespace HomeOS.Models.Modules;

// One module row on the manager page: its enable/disable state plus each
// permission it requested and whether the household currently grants it.
public class ModuleRowViewModel
{
    public IModuleDescriptor Descriptor { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public List<ModulePermissionRow> Permissions { get; set; } = new();
}

public record ModulePermissionRow(string Key, string DisplayName, bool IsGranted);

// One module row on a member's access page (RBAC): whether that member may
// currently open the module.
public record MemberModuleAccessRow(string Key, string DisplayName, string Icon, bool CanAccess);

public class ModuleManagementViewModel
{
    public List<ModuleRowViewModel> Modules { get; set; } = new();
}
