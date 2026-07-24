using HomeOS.Services;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Shopping;

// Shopping Lists registers itself in navigation/search/module-manager - see
// Docs/02_Pravila_Programiranja.md, section 1.2.
public class ShoppingListsModule : IModuleDescriptor
{
    private readonly IStringLocalizer<ShoppingListsModule> _localizer;

    public ShoppingListsModule(IStringLocalizer<ShoppingListsModule> localizer) => _localizer = localizer;

    public string Key => "ShoppingLists";
    public string Controller => "ShoppingLists";
    public string Icon => "🛒";
    public int SortOrder => 70;
    public string DisplayName => _localizer["NavLabel"];
}
