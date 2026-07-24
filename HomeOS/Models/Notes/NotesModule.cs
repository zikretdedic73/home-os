using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Notes;

// Notes registers itself in navigation/search/module-manager - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class NotesModule : IModuleDescriptor
{
    private readonly IStringLocalizer<NotesModule> _localizer;

    public NotesModule(IStringLocalizer<NotesModule> localizer) => _localizer = localizer;

    public string Key => "Notes";
    public string Controller => "Notes";
    public string Icon => "📝";
    public int SortOrder => 60;
    public string DisplayName => _localizer["NavLabel"];
}
