using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Models.Notes;
using HomeOS.Models.Tasks;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

[Authorize]
public class NotesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;
    private readonly IStringLocalizer<NotesController> _localizer;

    public NotesController(ApplicationDbContext context, ICurrentHouseholdService household, IStringLocalizer<NotesController> localizer)
    {
        _context = context;
        _household = household;
        _localizer = localizer;
    }

    // GET: /Notes
    public async Task<IActionResult> Index()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var notes = await _context.Notes
            .Where(n => n.HouseholdId == householdId && !n.IsDeleted)
            .VisibleTo(memberId)
            .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
            .OrderByDescending(n => n.UpdatedAtUtc ?? n.CreatedAtUtc)
            .ToListAsync();

        return View(notes);
    }

    // GET: /Notes/Create
    public async Task<IActionResult> Create()
    {
        await PopulateLinkOptionsAsync();
        return View(new Note());
    }

    // POST: /Notes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Note model, string? tagsInput, string? linkSelection)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        if (!ModelState.IsValid)
        {
            await PopulateLinkOptionsAsync();
            return View(model);
        }

        (model.LinkType, model.LinkedEntityId) = ParseLink(linkSelection);
        model.HouseholdId = householdId;
        model.OwnerId = await _household.GetCurrentMemberIdAsync();
        model.CreatedAtUtc = DateTime.UtcNow;
        if (model.IsJournalEntry && model.JournalDate == null)
            model.JournalDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _context.Notes.Add(model);
        await _context.SaveChangesAsync();
        await SaveTagsAsync(model, tagsInput, householdId);

        TempData["Success"] = _localizer["NoteCreatedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Notes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var note = await _context.Notes
            .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
            .FirstOrDefaultAsync(n => n.Id == id && n.HouseholdId == householdId);

        if (note == null) return NotFound();

        await PopulateLinkOptionsAsync();
        ViewBag.TagsInput = string.Join(", ", note.NoteTags.Where(nt => nt.Tag != null).Select(nt => nt.Tag!.Name));
        return View(note);
    }

    // POST: /Notes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Note model, string? tagsInput, string? linkSelection)
    {
        if (id != model.Id) return NotFound();

        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var note = await _context.Notes
            .Include(n => n.NoteTags)
            .FirstOrDefaultAsync(n => n.Id == id && n.HouseholdId == householdId);

        if (note == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateLinkOptionsAsync();
            return View(model);
        }

        var (linkType, linkedId) = ParseLink(linkSelection);
        note.Title = model.Title;
        note.Content = model.Content;
        note.LinkType = linkType;
        note.LinkedEntityId = linkedId;
        note.Visibility = model.Visibility;
        note.UpdatedAtUtc = DateTime.UtcNow;

        _context.NoteTags.RemoveRange(note.NoteTags);
        await _context.SaveChangesAsync();
        await SaveTagsAsync(note, tagsInput, householdId);

        TempData["Success"] = _localizer["NoteUpdatedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // POST: /Notes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var note = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id && n.HouseholdId == householdId);

        if (note != null)
        {
            note.IsDeleted = true;
            note.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = _localizer["NoteDeletedSuccessMessage"].Value;
        return RedirectToAction(nameof(Index));
    }

    // GET: /Notes/Journal - opens (or creates) today's journal entry for the
    // current member (one per day), then edits it.
    public async Task<IActionResult> Journal()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var entry = await _context.Notes.FirstOrDefaultAsync(n =>
            n.HouseholdId == householdId && n.OwnerId == memberId && !n.IsDeleted
            && n.IsJournalEntry && n.JournalDate == today);

        if (entry == null)
        {
            entry = new Note
            {
                HouseholdId = householdId,
                OwnerId = memberId,
                IsJournalEntry = true,
                JournalDate = today,
                Visibility = Visibility.Private, // journals default to private
                Content = string.Empty,
                CreatedAtUtc = DateTime.UtcNow
            };
            _context.Notes.Add(entry);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Edit), new { id = entry.Id });
    }

    // "Task:5" / "Event:3" / "" -> (LinkType, id). Keeps one dropdown in the
    // form instead of a JS-driven type+id cascade.
    private static (NoteLinkType, int?) ParseLink(string? linkSelection)
    {
        if (string.IsNullOrWhiteSpace(linkSelection) || !linkSelection.Contains(':'))
            return (NoteLinkType.None, null);

        var parts = linkSelection.Split(':', 2);
        if (!int.TryParse(parts[1], out var id))
            return (NoteLinkType.None, null);

        return parts[0] switch
        {
            "Task" => (NoteLinkType.Task, id),
            "Event" => (NoteLinkType.Event, id),
            _ => (NoteLinkType.None, null)
        };
    }

    private async Task PopulateLinkOptionsAsync()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        ViewBag.LinkableTasks = await _context.Tasks
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted)
            .VisibleTo(memberId)
            .OrderBy(t => t.Title)
            .ToListAsync();

        ViewBag.LinkableEvents = await _context.Events
            .Where(e => e.HouseholdId == householdId && !e.IsDeleted)
            .VisibleTo(memberId)
            .OrderBy(e => e.Title)
            .ToListAsync();
    }

    private async Task SaveTagsAsync(Note note, string? tagsInput, int householdId)
    {
        if (string.IsNullOrWhiteSpace(tagsInput)) return;

        var tagNames = tagsInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var name in tagNames)
        {
            // Reuse an existing household Tag (shared with Tasks) or create it.
            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.HouseholdId == householdId && t.Name == name);
            if (tag == null)
            {
                tag = new Tag { HouseholdId = householdId, Name = name };
                _context.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }

            _context.NoteTags.Add(new NoteTag { NoteId = note.Id, TagId = tag.Id });
        }

        await _context.SaveChangesAsync();
    }
}
