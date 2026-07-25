using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.LifeAdmin;

public class LifeAdminModule : IModuleDescriptor
{
    private readonly IStringLocalizer<LifeAdminModule> _localizer;
    public LifeAdminModule(IStringLocalizer<LifeAdminModule> localizer) => _localizer = localizer;

    public string Key => "LifeAdmin";
    public string Controller => "LifeAdmin";
    public string Icon => "🗂️";
    public int SortOrder => 80;
    public string DisplayName => _localizer["NavLabel"];
}
