using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Models.Events;
using HomeOS.Models.LifeAdmin;
using HomeOS.Services;
using HomeOS.Services.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

[Authorize]
public class LifeAdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;
    private readonly IEventBus _eventBus;
    private readonly IStringLocalizer<LifeAdminController> _localizer;

    public LifeAdminController(
        ApplicationDbContext context,
        ICurrentHouseholdService household,
        IEventBus eventBus,
        IStringLocalizer<LifeAdminController> localizer)
    {
        _context = context;
        _household = household;
        _eventBus = eventBus;
        _localizer = localizer;
    }

    // GET: /LifeAdmin - documents + contacts side by side.
    public async Task<IActionResult> Index()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        ViewBag.Documents = await _context.Documents
            .Where(d => d.HouseholdId == householdId && !d.IsDeleted)
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Document)
            .OrderBy(d => d.ExpiryDate == null).ThenBy(d => d.ExpiryDate)
            .ToListAsync();

        ViewBag.Contacts = await _context.Contacts
            .Where(c => c.HouseholdId == householdId && !c.IsDeleted)
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Document)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View();
    }

    // ---- Documents ----

    public IActionResult CreateDocument() => View(new Document());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDocument(Document model)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        if (!ModelState.IsValid) return View(model);

        model.HouseholdId = householdId;
        model.OwnerId = await _household.GetCurrentMemberIdAsync();
        model.CreatedAtUtc = DateTime.UtcNow;

        _context.Documents.Add(model);
        await _context.SaveChangesAsync();

        // Has an expiry -> let Reminders schedule the renewal warning.
        if (model.ExpiryDate.HasValue)
        {
            await _eventBus.PublishAsync(new DocumentExpiryCreatedEvent(
                householdId, model.Id, model.OwnerId, model.Name,
                model.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue)));
        }

        TempData["Success"] = _localizer["DocumentSaved"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditDocument(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var doc = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id && d.HouseholdId == householdId && !d.IsDeleted);
        if (doc == null) return NotFound();
        return View(doc);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDocument(int id, Document model)
    {
        if (id != model.Id) return NotFound();
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var doc = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id && d.HouseholdId == householdId && !d.IsDeleted);
        if (doc == null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        var expiryChanged = doc.ExpiryDate != model.ExpiryDate;

        doc.Name = model.Name;
        doc.Category = model.Category;
        doc.ExpiryDate = model.ExpiryDate;
        doc.Notes = model.Notes;
        doc.Visibility = model.Visibility;
        doc.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // A newly set/changed expiry schedules a (fresh) renewal reminder; the
        // handler is idempotent per document so an unchanged date won't double up.
        if (model.ExpiryDate.HasValue && expiryChanged)
        {
            await _eventBus.PublishAsync(new DocumentExpiryCreatedEvent(
                householdId, doc.Id, doc.OwnerId, doc.Name,
                model.ExpiryDate.Value.ToDateTime(TimeOnly.MinValue)));
        }

        TempData["Success"] = _localizer["DocumentSaved"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var doc = await _context.Documents
            .FirstOrDefaultAsync(d => d.Id == id && d.HouseholdId == householdId);
        if (doc != null)
        {
            doc.IsDeleted = true;
            doc.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // ---- Contacts ----

    public IActionResult CreateContact() => View(new Contact());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateContact(Contact model)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        if (!ModelState.IsValid) return View(model);

        model.HouseholdId = householdId;
        model.OwnerId = await _household.GetCurrentMemberIdAsync();
        model.CreatedAtUtc = DateTime.UtcNow;

        _context.Contacts.Add(model);
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["ContactSaved"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditContact(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.HouseholdId == householdId && !c.IsDeleted);
        if (contact == null) return NotFound();
        return View(contact);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditContact(int id, Contact model)
    {
        if (id != model.Id) return NotFound();
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.HouseholdId == householdId && !c.IsDeleted);
        if (contact == null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        contact.Name = model.Name;
        contact.Role = model.Role;
        contact.Phone = model.Phone;
        contact.Email = model.Email;
        contact.Notes = model.Notes;
        contact.Visibility = model.Visibility;
        contact.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["ContactSaved"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var contact = await _context.Contacts
            .FirstOrDefaultAsync(c => c.Id == id && c.HouseholdId == householdId);
        if (contact != null)
        {
            contact.IsDeleted = true;
            contact.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
