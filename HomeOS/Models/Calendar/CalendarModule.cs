using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Calendar;

// Calendar registers itself in navigation/search/module-manager - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class CalendarModule : IModuleDescriptor
{
    private readonly IStringLocalizer<CalendarModule> _localizer;

    public CalendarModule(IStringLocalizer<CalendarModule> localizer) => _localizer = localizer;

    public string Key => "Calendar";
    public string Controller => "Calendar";
    public string Icon => "📅";
    public int SortOrder => 40;
    public string DisplayName => _localizer["NavLabel"];
}
