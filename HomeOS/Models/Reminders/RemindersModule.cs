using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Reminders;

// Reminders registers itself in navigation/search/module-manager - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class RemindersModule : IModuleDescriptor
{
    private readonly IStringLocalizer<RemindersModule> _localizer;

    public RemindersModule(IStringLocalizer<RemindersModule> localizer) => _localizer = localizer;

    public string Key => "Reminders";
    public string Controller => "Reminders";
    public string Icon => "🔔";
    public int SortOrder => 50;
    public string DisplayName => _localizer["NavLabel"];
}
