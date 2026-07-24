using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

// Shell-owned module manager - lets a household "install/uninstall" (enable/
// disable) modules. The list is generated from the registry, so it grows
// automatically as modules are added (Docs/00_Specifikacija_Izvor.md,
// "Instalacija i uklanjanje su čisti i reverzibilni").
[Authorize]
public class ModulesController : Controller
{
    private readonly IModuleRegistry _registry;
    private readonly ICurrentHouseholdService _household;
    private readonly IStringLocalizer<ModulesController> _localizer;

    public ModulesController(IModuleRegistry registry, ICurrentHouseholdService household, IStringLocalizer<ModulesController> localizer)
    {
        _registry = registry;
        _household = household;
        _localizer = localizer;
    }

    // GET: /Modules
    public async Task<IActionResult> Index()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var modules = await _registry.GetAllAsync(householdId);
        return View(modules);
    }

    // POST: /Modules/Toggle
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(string key, bool enabled)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        await _registry.SetEnabledAsync(householdId, key, enabled);

        TempData["Success"] = _localizer["ModuleStateUpdatedMessage"].Value;
        return RedirectToAction(nameof(Index));
    }
}
