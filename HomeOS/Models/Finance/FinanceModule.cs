using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Finance;

// Self-registering descriptor - Finance appears in nav/command palette/module
// manager purely from this + its Program.cs line, no Shell changes.
public class FinanceModule : IModuleDescriptor
{
    private readonly IStringLocalizer<FinanceModule> _localizer;
    public FinanceModule(IStringLocalizer<FinanceModule> localizer) => _localizer = localizer;

    public string Key => "Finance";
    public string Controller => "Finance";
    public string Icon => "💰";
    public int SortOrder => 70;
    public string DisplayName => _localizer["NavLabel"];
}
