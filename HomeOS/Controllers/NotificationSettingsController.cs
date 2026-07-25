using HomeOS.Models.Notifications;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

// Per-member notification settings - each member turns email categories on/off
// for themselves (Docs/00 - "Individualna podešavanja obavještenja",
// "uključivanje/isključivanje kategorija obavještenja"). Settings are personal,
// so everything is scoped to the current member, not the household.
[Authorize]
public class NotificationSettingsController : Controller
{
    private readonly ICurrentHouseholdService _household;
    private readonly INotificationPreferenceService _preferences;
    private readonly IStringLocalizer<NotificationSettingsController> _localizer;

    public NotificationSettingsController(
        ICurrentHouseholdService household,
        INotificationPreferenceService preferences,
        IStringLocalizer<NotificationSettingsController> localizer)
    {
        _household = household;
        _preferences = preferences;
        _localizer = localizer;
    }

    // GET: /NotificationSettings
    public async Task<IActionResult> Index()
    {
        var memberId = await _household.GetCurrentMemberIdAsync();
        var settings = await _preferences.GetSettingsAsync(memberId);
        return View(settings);
    }

    // POST: /NotificationSettings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(NotificationCategory[]? enabledCategories)
    {
        var memberId = await _household.GetCurrentMemberIdAsync();
        var enabled = (enabledCategories ?? Array.Empty<NotificationCategory>()).ToHashSet();

        // An unchecked box simply doesn't post, so a category missing from the
        // submitted set means "off". Persist every known category explicitly.
        foreach (var category in Enum.GetValues<NotificationCategory>())
        {
            await _preferences.SetAsync(memberId, category, enabled.Contains(category));
        }

        TempData["Success"] = _localizer["SavedMessage"].Value;
        return RedirectToAction(nameof(Index));
    }
}
