using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Tasks;

// Tasks registers itself in the navigation/search/module-manager (see
// Docs/02_Pravila_Programiranja.md, section 1.2). Doubles as its own
// localization marker for the nav label (TasksModule.{culture}.resx).
public class TasksModule : IModuleDescriptor
{
    private readonly IStringLocalizer<TasksModule> _localizer;

    public TasksModule(IStringLocalizer<TasksModule> localizer) => _localizer = localizer;

    public string Key => "Tasks";
    public string Controller => "Tasks";
    public string Icon => "✅";
    public int SortOrder => 20;
    public string DisplayName => _localizer["NavLabel"];
}
