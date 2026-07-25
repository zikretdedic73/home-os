using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Models.Shopping;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Controllers;

[Authorize]
public class ShoppingListsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;

    public ShoppingListsController(ApplicationDbContext context, ICurrentHouseholdService household)
    {
        _context = context;
        _household = household;
    }

    // GET: /ShoppingLists
    public async Task<IActionResult> Index()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var lists = await _context.ShoppingLists
            .Where(l => l.HouseholdId == householdId && !l.IsDeleted)
            .VisibleTo(memberId, _context.ItemShares, ShareableType.ShoppingList)
            .Include(l => l.Items)
            .OrderBy(l => l.Name)
            .ToListAsync();

        return View(lists);
    }

    // GET: /ShoppingLists/List/5
    public async Task<IActionResult> List(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var list = await _context.ShoppingLists
            .Include(l => l.Items.OrderBy(i => i.IsChecked).ThenBy(i => i.Id))
            .FirstOrDefaultAsync(l => l.Id == id && l.HouseholdId == householdId && !l.IsDeleted);

        if (list == null) return NotFound();
        return View(list);
    }

    // POST: /ShoppingLists/CreateList
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateList(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return RedirectToAction(nameof(Index));

        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var list = new ShoppingList { HouseholdId = householdId, OwnerId = memberId, Name = name.Trim() };
        _context.ShoppingLists.Add(list);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(List), new { id = list.Id });
    }

    // POST: /ShoppingLists/DeleteList
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteList(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var list = await _context.ShoppingLists.FirstOrDefaultAsync(l => l.Id == id && l.HouseholdId == householdId);

        if (list != null)
        {
            list.IsDeleted = true;
            list.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /ShoppingLists/AddItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int listId, string name, string? quantity)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();
        var list = await _context.ShoppingLists.FirstOrDefaultAsync(l => l.Id == listId && l.HouseholdId == householdId && !l.IsDeleted);

        if (list != null && !string.IsNullOrWhiteSpace(name))
        {
            _context.ShoppingListItems.Add(new ShoppingListItem
            {
                ShoppingListId = listId,
                Name = name.Trim(),
                Quantity = string.IsNullOrWhiteSpace(quantity) ? null : quantity.Trim(),
                AddedByMemberId = memberId
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(List), new { id = listId });
    }

    // POST: /ShoppingLists/RemoveItem
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(int itemId, int listId)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var item = await _context.ShoppingListItems
            .Include(i => i.ShoppingList)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item?.ShoppingList != null && item.ShoppingList.HouseholdId == householdId)
        {
            _context.ShoppingListItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(List), new { id = listId });
    }

    // POST: /ShoppingLists/ToggleItem - AJAX check/uncheck without reload.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleItem(int itemId)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var item = await _context.ShoppingListItems
            .Include(i => i.ShoppingList)
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item?.ShoppingList == null || item.ShoppingList.HouseholdId != householdId)
            return Json(new { success = false });

        item.IsChecked = !item.IsChecked;
        await _context.SaveChangesAsync();

        return Json(new { success = true, isChecked = item.IsChecked });
    }
}
