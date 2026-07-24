using HomeOS.Models.Modules;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

// Shell-owned module manager - lets a household "install/uninstall" (enable/
// disable) modules and review/revoke the data-access permissions each module
// requested. The list is generated from the registry, so it grows
// automatically as modules are added (Docs/00_Specifikacija_Izvor.md,
// "Instalacija i uklanjanje su čisti i reverzibilni" + "Kontrola i privatnost").
[Authorize]
public class ModulesController : Controller
{
    private readonly IModuleRegistry _registry;
    private readonly IPermissionService _permissions;
    private readonly ICurrentHouseholdService _household;
    private readonly IStringLocalizer<ModulesController> _localizer;

    public ModulesController(IModuleRegistry registry, IPermissionService permissions, ICurrentHouseholdService household, IStringLocalizer<ModulesController> localizer)
    {
        _registry = registry;
        _permissions = permissions;
        _household = household;
        _localizer = localizer;
    }

    // GET: /Modules
    public async Task<IActionResult> Index()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var modules = await _registry.GetAllAsync(householdId);

        var viewModel = new ModuleManagementViewModel();
        foreach (var module in modules)
        {
            var permissionRows = new List<ModulePermissionRow>();
            foreach (var perm in module.Descriptor.RequestedPermissions)
            {
                var granted = await _permissions.HasPermissionAsync(householdId, module.Descriptor.Key, perm.Key);
                permissionRows.Add(new ModulePermissionRow(perm.Key, perm.DisplayName, granted));
            }

            viewModel.Modules.Add(new ModuleRowViewModel
            {
                Descriptor = module.Descriptor,
                IsEnabled = module.IsEnabled,
                Permissions = permissionRows
            });
        }

        return View(viewModel);
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

    // POST: /Modules/TogglePermission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePermission(string key, string permission, bool granted)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        await _permissions.SetPermissionAsync(householdId, key, permission, granted);

        TempData["Success"] = _localizer["PermissionUpdatedMessage"].Value;
        return RedirectToAction(nameof(Index));
    }
}
