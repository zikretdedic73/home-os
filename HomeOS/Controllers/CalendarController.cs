using HomeOS.Data;
using HomeOS.Models.Calendar;
using HomeOS.Models.Households;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

[Authorize]
public class CalendarController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;
    private readonly IStringLocalizer<CalendarController> _localizer;

    public CalendarController(ApplicationDbContext context, ICurrentHouseholdService household, IStringLocalizer<CalendarController> localizer)
    {
        _context = context;
        _household = household;
        _localizer = localizer;
    }

    // GET: /Calendar
    public IActionResult Index() => View();

    // GET: /Calendar/Events?start=...&end=... - FullCalendar JSON event source.
    // Combines the Event table with a read-only projection of TaskItem.DueDate
    // (no duplication - see Docs/04_Model_Podataka.md, section 5).
    [HttpGet]
    public async Task<IActionResult> Events(DateTime start, DateTime end)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var events = await _context.Events
            .Where(e => e.HouseholdId == householdId && !e.IsDeleted
                && e.StartsAtUtc < end && e.EndsAtUtc > start)
            .Select(e => new
            {
                id = $"event-{e.Id}",
                title = e.Title,
                start = e.StartsAtUtc,
                end = e.EndsAtUtc,
                url = Url.Action(nameof(Edit), new { id = e.Id }),
                color = "#2f6f63"
            })
            .ToListAsync();

        var tasks = await _context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted
                && t.DueDate != null && t.DueDate >= start && t.DueDate < end)
            .Select(t => new
            {
                id = $"task-{t.Id}",
                title = t.Title,
                start = t.DueDate!.Value,
                end = (DateTime?)null,
                url = Url.Action("Edit", "Tasks", new { id = t.Id }),
                color = "#d97706"
            })
            .ToListAsync();

        return Json(events.Cast<object>().Concat(tasks.Cast<object>()));
    }

    // GET: /Calendar/Create
    public async Task<IActionResult> Create(DateTime? date)
    {
        ViewBag.Members = await GetMembersAsync(await _household.GetCurrentHouseholdIdAsync());
        var start = date ?? DateTime.UtcNow;
        return View(new Event { StartsAtUtc = start, EndsAtUtc = start.AddHours(1) });
    }

    // POST: /Calendar/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event model, int[]? attendeeMemberIds)
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

        _context.Events.Add(model);
        await _context.SaveChangesAsync();

        await SaveAttendeesAsync(model.Id, attendeeMemberIds);

        TempData["Success"] = _localizer["EventCreatedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Calendar/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var eventItem = await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id && e.HouseholdId == householdId);

        if (eventItem == null) return NotFound();

        ViewBag.Members = await GetMembersAsync(householdId);
        ViewBag.SelectedMemberIds = eventItem.Attendees.Select(a => a.MemberId).ToList();
        return View(eventItem);
    }

    // POST: /Calendar/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event model, int[]? attendeeMemberIds)
    {
        if (id != model.Id) return NotFound();

        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var eventItem = await _context.Events
            .Include(e => e.Attendees)
            .FirstOrDefaultAsync(e => e.Id == id && e.HouseholdId == householdId);

        if (eventItem == null) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.Members = await GetMembersAsync(householdId);
            ViewBag.SelectedMemberIds = eventItem.Attendees.Select(a => a.MemberId).ToList();
            return View(model);
        }

        eventItem.Title = model.Title;
        eventItem.StartsAtUtc = model.StartsAtUtc;
        eventItem.EndsAtUtc = model.EndsAtUtc;
        eventItem.Location = model.Location;
        eventItem.RecurrenceRule = model.RecurrenceRule;
        eventItem.UpdatedAtUtc = DateTime.UtcNow;

        _context.EventAttendees.RemoveRange(eventItem.Attendees);
        await _context.SaveChangesAsync();
        await SaveAttendeesAsync(eventItem.Id, attendeeMemberIds);

        TempData["Success"] = _localizer["EventUpdatedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Calendar/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.HouseholdId == householdId);

        if (eventItem == null) return NotFound();
        return View(eventItem);
    }

    // POST: /Calendar/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var eventItem = await _context.Events.FirstOrDefaultAsync(e => e.Id == id && e.HouseholdId == householdId);

        if (eventItem != null)
        {
            eventItem.IsDeleted = true;
            eventItem.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = _localizer["EventDeletedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<Member>> GetMembersAsync(int householdId)
    {
        return await _context.Members
            .Where(m => m.HouseholdId == householdId)
            .OrderBy(m => m.DisplayName)
            .ToListAsync();
    }

    private async Task SaveAttendeesAsync(int eventId, int[]? attendeeMemberIds)
    {
        if (attendeeMemberIds == null || attendeeMemberIds.Length == 0)
            return;

        foreach (var memberId in attendeeMemberIds.Distinct())
        {
            _context.EventAttendees.Add(new EventAttendee { EventId = eventId, MemberId = memberId });
        }

        await _context.SaveChangesAsync();
    }
}
