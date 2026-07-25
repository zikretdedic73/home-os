using HomeOS.Data;
using HomeOS.Models.Common;
using HomeOS.Models.Events;
using HomeOS.Models.Finance;
using HomeOS.Models.Households;
using HomeOS.Services;
using HomeOS.Services.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers;

[Authorize]
public class FinanceController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentHouseholdService _household;
    private readonly IEventBus _eventBus;
    private readonly IItemSharingService _sharing;
    private readonly IStringLocalizer<FinanceController> _localizer;

    public FinanceController(
        ApplicationDbContext context,
        ICurrentHouseholdService household,
        IEventBus eventBus,
        IItemSharingService sharing,
        IStringLocalizer<FinanceController> localizer)
    {
        _context = context;
        _household = household;
        _eventBus = eventBus;
        _sharing = sharing;
        _localizer = localizer;
    }

    // GET: /Finance - current-month overview (summary per category vs budget,
    // income/expense totals, recent transactions, bills).
    public async Task<IActionResult> Index(int? year, int? month)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();

        var now = DateTime.UtcNow;
        var y = year ?? now.Year;
        var m = month ?? now.Month;
        var from = new DateOnly(y, m, 1);
        var to = from.AddMonths(1);

        var monthTx = await _context.Transactions
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted
                && t.OccurredOn >= from && t.OccurredOn < to)
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Transaction)
            .Include(t => t.Category)
            .ToListAsync();

        var categories = await _context.Categories
            .Where(c => c.HouseholdId == householdId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        var budgets = await _context.Budgets
            .Where(b => b.HouseholdId == householdId)
            .ToListAsync();

        var summaries = categories.Select(c =>
        {
            var spent = monthTx
                .Where(t => t.Type == TransactionType.Expense && t.CategoryId == c.Id)
                .Sum(t => t.Amount);
            var limit = budgets.FirstOrDefault(b => b.CategoryId == c.Id)?.MonthlyLimit;
            return new CategorySummary { Category = c, Spent = spent, Limit = limit };
        }).ToList();

        var recent = await _context.Transactions
            .Where(t => t.HouseholdId == householdId && !t.IsDeleted)
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Transaction)
            .Include(t => t.Category)
            .OrderByDescending(t => t.OccurredOn).ThenByDescending(t => t.Id)
            .Take(10)
            .ToListAsync();

        var bills = await _context.Bills
            .Where(b => b.HouseholdId == householdId && !b.IsDeleted)
            .VisibleTo(memberId, _context.ItemShares, ShareableType.Bill)
            .OrderBy(b => b.IsPaid).ThenBy(b => b.DueDate)
            .ToListAsync();

        var model = new FinanceOverviewViewModel
        {
            Year = y,
            Month = m,
            TotalIncome = monthTx.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
            TotalExpense = monthTx.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
            Categories = summaries,
            RecentTransactions = recent,
            Bills = bills,
            Members = await GetMembersAsync(householdId)
        };

        return View(model);
    }

    // ---- Categories & budgets (managed inline from the overview) ----

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(string name)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        if (!string.IsNullOrWhiteSpace(name))
        {
            var exists = await _context.Categories
                .AnyAsync(c => c.HouseholdId == householdId && c.Name == name.Trim());
            if (!exists)
            {
                _context.Categories.Add(new Category { HouseholdId = householdId, Name = name.Trim() });
                await _context.SaveChangesAsync();
            }
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetBudget(int categoryId, decimal monthlyLimit)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.HouseholdId == householdId);
        if (category != null)
        {
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.HouseholdId == householdId && b.CategoryId == categoryId);
            if (budget == null)
            {
                budget = new Budget { HouseholdId = householdId, CategoryId = categoryId };
                _context.Budgets.Add(budget);
            }
            budget.MonthlyLimit = monthlyLimit;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // ---- Transactions ----

    public async Task<IActionResult> CreateTransaction()
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        await PopulateTransactionFormAsync(householdId);
        return View(new Transaction { OccurredOn = DateOnly.FromDateTime(DateTime.UtcNow) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTransaction(Transaction model, int[]? splitMemberIds, decimal[]? splitAmounts)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        if (!ModelState.IsValid)
        {
            await PopulateTransactionFormAsync(householdId);
            return View(model);
        }

        model.HouseholdId = householdId;
        model.OwnerId = await _household.GetCurrentMemberIdAsync();
        model.CreatedAtUtc = DateTime.UtcNow;

        _context.Transactions.Add(model);
        await _context.SaveChangesAsync();

        await SaveSharesAsync(model, splitMemberIds, splitAmounts);
        await SaveVisibilitySharesAsync(model);

        TempData["Success"] = _localizer["TransactionSaved"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditTransaction(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var tx = await _context.Transactions
            .Include(t => t.Shares)
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId && !t.IsDeleted);
        if (tx == null) return NotFound();

        await PopulateTransactionFormAsync(householdId);
        ViewBag.ShareMemberIds = await _sharing.GetShareMemberIdsAsync(ShareableType.Transaction, tx.Id);
        return View(tx);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTransaction(int id, Transaction model, int[]? splitMemberIds, decimal[]? splitAmounts)
    {
        if (id != model.Id) return NotFound();
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var tx = await _context.Transactions
            .Include(t => t.Shares)
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId && !t.IsDeleted);
        if (tx == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await PopulateTransactionFormAsync(householdId);
            ViewBag.ShareMemberIds = await _sharing.GetShareMemberIdsAsync(ShareableType.Transaction, tx.Id);
            return View(model);
        }

        tx.Description = model.Description;
        tx.Amount = model.Amount;
        tx.Type = model.Type;
        tx.CategoryId = model.CategoryId;
        tx.OccurredOn = model.OccurredOn;
        tx.Visibility = model.Visibility;
        tx.UpdatedAtUtc = DateTime.UtcNow;

        _context.ExpenseShares.RemoveRange(tx.Shares);
        await _context.SaveChangesAsync();

        await SaveSharesAsync(tx, splitMemberIds, splitAmounts);
        await SaveVisibilitySharesAsync(tx);

        TempData["Success"] = _localizer["TransactionSaved"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var tx = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == id && t.HouseholdId == householdId);
        if (tx != null)
        {
            tx.IsDeleted = true;
            tx.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // ---- Bills ----

    public IActionResult CreateBill()
    {
        return View(new Bill { DueDate = DateOnly.FromDateTime(DateTime.UtcNow) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateBill(Bill model)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        if (!ModelState.IsValid) return View(model);

        model.HouseholdId = householdId;
        model.OwnerId = await _household.GetCurrentMemberIdAsync();
        model.CreatedAtUtc = DateTime.UtcNow;

        _context.Bills.Add(model);
        await _context.SaveChangesAsync();

        // Let Reminders schedule the "bill about to be charged" warning.
        await _eventBus.PublishAsync(new BillDueDateCreatedEvent(
            householdId, model.Id, model.OwnerId, model.Name,
            model.DueDate.ToDateTime(TimeOnly.MinValue)));

        TempData["Success"] = _localizer["BillSaved"].Value;
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditBill(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == id && b.HouseholdId == householdId && !b.IsDeleted);
        if (bill == null) return NotFound();
        return View(bill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditBill(int id, Bill model)
    {
        if (id != model.Id) return NotFound();
        var householdId = await _household.GetCurrentHouseholdIdAsync();

        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == id && b.HouseholdId == householdId && !b.IsDeleted);
        if (bill == null) return NotFound();
        if (!ModelState.IsValid) return View(model);

        bill.Name = model.Name;
        bill.Amount = model.Amount;
        bill.DueDate = model.DueDate;
        bill.RecurrenceRule = model.RecurrenceRule;
        bill.Visibility = model.Visibility;
        bill.UpdatedAtUtc = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = _localizer["BillSaved"].Value;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkBillPaid(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == id && b.HouseholdId == householdId && !b.IsDeleted);
        if (bill != null)
        {
            bill.IsPaid = true;
            bill.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBill(int id)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var bill = await _context.Bills
            .FirstOrDefaultAsync(b => b.Id == id && b.HouseholdId == householdId);
        if (bill != null)
        {
            bill.IsDeleted = true;
            bill.UpdatedAtUtc = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    // ---- helpers ----

    private async Task PopulateTransactionFormAsync(int householdId)
    {
        ViewBag.Categories = await _context.Categories
            .Where(c => c.HouseholdId == householdId)
            .OrderBy(c => c.Name)
            .ToListAsync();
        ViewBag.Members = await GetMembersAsync(householdId);
    }

    private async Task<List<Member>> GetMembersAsync(int householdId)
    {
        return await _context.Members
            .Where(m => m.HouseholdId == householdId)
            .OrderBy(m => m.DisplayName)
            .ToListAsync();
    }

    // Persists per-member split rows (only meaningful for expenses). Pairs
    // splitMemberIds[i] with splitAmounts[i]; ignores zero/blank amounts.
    private async Task SaveSharesAsync(Transaction tx, int[]? memberIds, decimal[]? amounts)
    {
        if (memberIds == null || amounts == null) return;

        var count = Math.Min(memberIds.Length, amounts.Length);
        for (var i = 0; i < count; i++)
        {
            if (amounts[i] <= 0) continue;
            _context.ExpenseShares.Add(new ExpenseShare
            {
                TransactionId = tx.Id,
                MemberId = memberIds[i],
                Amount = amounts[i]
            });
        }
        await _context.SaveChangesAsync();
    }

    // Mirrors the Tasks pattern: specific-member visibility shares are read from
    // the posted checkboxes only when Visibility == SpecificMembers.
    private async Task SaveVisibilitySharesAsync(Transaction tx)
    {
        var ids = Request.Form["shareMemberIds"]
            .Select(v => int.TryParse(v, out var n) ? n : (int?)null)
            .Where(n => n.HasValue).Select(n => n!.Value);

        var members = tx.Visibility == Visibility.SpecificMembers ? ids : Enumerable.Empty<int>();
        await _sharing.ReplaceSharesAsync(ShareableType.Transaction, tx.Id, members);
    }
}
