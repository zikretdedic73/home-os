using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Calendar;

// Calendar registers itself in navigation/search/module-manager - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class CalendarModule : IModuleDescriptor
{
    private readonly IStringLocalizer<CalendarModule> _localizer;

    public CalendarModule(IStringLocalizer<CalendarModule> localizer) => _localizer = localizer;

    public const string Key_Static = "Calendar";

    public string Key => Key_Static;
    public string Controller => "Calendar";
    public string Icon => "📅";
    public int SortOrder => 40;
    public string DisplayName => _localizer["NavLabel"];

    // Calendar projects task deadlines onto the calendar - it needs to read
    // the Tasks module. The household can revoke this (see PermissionService),
    // after which the calendar shows only its own events.
    public const string TasksReadPermission = "Tasks.Read";

    public IReadOnlyList<ModulePermission> RequestedPermissions => new[]
    {
        new ModulePermission(TasksReadPermission, _localizer["Perm_TasksRead"])
    };
}
