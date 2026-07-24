using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Models.Households;
using HomeOS.Models.Reminders;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

[Authorize]
public class RemindersController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;
    private readonly IStringLocalizer<RemindersController> _localizer;

    public RemindersController(ApplicationDbContext context, ICurrentHouseholdService household, IStringLocalizer<RemindersController> localizer)
    {
        _context = context;
        _household = household;
        _localizer = localizer;
    }

    // GET: /Reminders
    public async Task<IActionResult> Index()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var reminders = await _context.Reminders
            .Where(r => r.HouseholdId == householdId && !r.IsDeleted)
            .VisibleTo(memberId)
            .Include(r => r.Recipients)
            .OrderBy(r => r.IsResolved)
            .ThenBy(r => r.TriggerAtUtc)
            .ToListAsync();

        ViewBag.Members = await GetMembersAsync(householdId);
        return View(reminders);
    }

    // GET: /Reminders/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.Members = await GetMembersAsync(await _household.GetCurrentHouseholdIdAsync());
        return View(new Reminder());
    }

    // POST: /Reminders/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Reminder model, int[]? recipientMemberIds)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        if (!ModelState.IsValid)
        {
            ViewBag.Members = await GetMembersAsync(householdId);
            return View(model);
        }

        model.HouseholdId = householdId;
        model.OwnerId = await _household.GetCurrentMemberIdAsync();
        model.CreatedAtUtc = DateTime.UtcNow;
        model.SourceType = ReminderSourceType.Manual;

        _context.Reminders.Add(model);
        await _context.SaveChangesAsync();

        await SaveRecipientsAsync(model.Id, recipientMemberIds, model.OwnerId);

        TempData["Success"] = _localizer["ReminderCreatedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Reminders/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var reminder = await _context.Reminders
            .Include(r => r.Recipients)
            .FirstOrDefaultAsync(r => r.Id == id && r.HouseholdId == householdId);

        if (reminder == null) return NotFound();

        ViewBag.Members = await GetMembersAsync(householdId);
        ViewBag.SelectedMemberIds = reminder.Recipients.Select(r => r.MemberId).ToList();
        return View(reminder);
    }

    // POST: /Reminders/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Reminder model, int[]? recipientMemberIds)
    {
        if (id != model.Id) return NotFound();

        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var reminder = await _context.Reminders
            .Include(r => r.Recipients)
            .FirstOrDefaultAsync(r => r.Id == id && r.HouseholdId == householdId);

        if (reminder == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Members = await GetMembersAsync(householdId);
            ViewBag.SelectedMemberIds = reminder.Recipients.Select(r => r.MemberId).ToList();
            return View(model);
        }

        reminder.Title = model.Title;
        reminder.TriggerAtUtc = model.TriggerAtUtc;
        reminder.RecurrenceRule = model.RecurrenceRule;
        reminder.Visibility = model.Visibility;
        reminder.UpdatedAtUtc = DateTime.UtcNow;

        _context.ReminderRecipients.RemoveRange(reminder.Recipients);
        await _context.SaveChangesAsync();
        await SaveRecipientsAsync(reminder.Id, recipientMemberIds, reminder.OwnerId);

        TempData["Success"] = _localizer["ReminderUpdatedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // POST: /Reminders/Resolve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.HouseholdId == householdId);

        if (reminder != null)
        {
            reminder.IsResolved = true;
            reminder.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Reminders/Snooze/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Snooze(int id, int hours = 1)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.HouseholdId == householdId);

        if (reminder != null)
        {
            reminder.SnoozedUntilUtc = DateTime.UtcNow.AddHours(hours);
            reminder.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: /Reminders/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.HouseholdId == householdId);

        if (reminder == null) return NotFound();
        return View(reminder);
    }

    // POST: /Reminders/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var reminder = await _context.Reminders.FirstOrDefaultAsync(r => r.Id == id && r.HouseholdId == householdId);

        if (reminder != null)
        {
            reminder.IsDeleted = true;
            reminder.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = _localizer["ReminderDeletedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Member>> GetMembersAsync(int householdId)
    {
        return await _context.Members
            .Where(m => m.HouseholdId == householdId)
            .OrderBy(m => m.DisplayName)
            .ToListAsync();
    }

    private async Task SaveRecipientsAsync(int reminderId, int[]? recipientMemberIds, int fallbackMemberId)
    {
        var memberIds = (recipientMemberIds != null && recipientMemberIds.Length > 0)
            ? recipientMemberIds
            : new[] { fallbackMemberId };

        foreach (var memberId in memberIds.Distinct())
        {
            _context.ReminderRecipients.Add(new ReminderRecipient { ReminderId = reminderId, MemberId = memberId });
        }

        await _context.SaveChangesAsync();
    }
}
