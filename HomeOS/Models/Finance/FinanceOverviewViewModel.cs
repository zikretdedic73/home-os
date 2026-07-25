using HomeOS.Models.Households;

namespace HomeOS.Models.Finance;

// Everything the Finance overview (Index) needs for the current month: per-
// category spend vs budget, income/expense totals, recent entries and bills.
public class FinanceOverviewViewModel
{
    public int Year { get; set; }
    public int Month { get; set; }

    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Net => TotalIncome - TotalExpense;

    public List<CategorySummary> Categories { get; set; } = new();
    public List<Transaction> RecentTransactions { get; set; } = new();
    public List<Bill> Bills { get; set; } = new();
    public List<Member> Members { get; set; } = new();
}

// One category's spend for the month against its optional monthly budget.
public class CategorySummary
{
    public Category Category { get; set; } = null!;
    public decimal Spent { get; set; }
    public decimal? Limit { get; set; }

    public bool OverBudget => Limit.HasValue && Spent > Limit.Value;
    // Percentage of the budget used (0 when no budget set), capped for the bar.
    public int PercentUsed => Limit is > 0 ? (int)Math.Min(100, Math.Round(Spent / Limit.Value * 100)) : 0;
}
