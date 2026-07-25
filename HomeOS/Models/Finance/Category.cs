using System.ComponentModel.DataAnnotations;

namespace HomeOS.Models.Finance;

// A spending/income category, scoped to the household (like Tag - shared
// classification, not per-owner content, so it does not extend BaseEntity or
// carry its own visibility).
public class Category
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
