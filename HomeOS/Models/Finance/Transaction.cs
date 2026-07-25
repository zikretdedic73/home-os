using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HomeOS.Models.Common;

namespace HomeOS.Models.Finance;

// A single income or expense entry. Extends BaseEntity so it honors household
// isolation and the same visibility rules as everything else (a member can keep
// a transaction private, share it with the household, or with specific people).
public class Transaction : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public TransactionType Type { get; set; } = TransactionType.Expense;

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // Date the money moved (no time-of-day needed for a ledger entry).
    public DateOnly OccurredOn { get; set; }

    // Optional split of this expense among members (Docs/01 - split expense).
    public ICollection<ExpenseShare> Shares { get; set; } = new List<ExpenseShare>();
}
