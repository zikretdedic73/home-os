using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Kanban;

// Kanban registers itself in navigation/search/module-manager - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class KanbanModule : IModuleDescriptor
{
    private readonly IStringLocalizer<KanbanModule> _localizer;

    public KanbanModule(IStringLocalizer<KanbanModule> localizer) => _localizer = localizer;

    public string Key => "Kanban";
    public string Controller => "Kanban";
    public string Icon => "📋";
    public int SortOrder => 30;
    public string DisplayName => _localizer["NavLabel"];
}
