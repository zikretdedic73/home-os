using System.ComponentModel.DataAnnotations.Schema;

namespace HomeOS.Models.Finance;

// A monthly spending limit for a category, scoped to the household. The monthly
// summary compares this against the sum of that category's expenses in the
// current month. One budget per (household, category).
public class Budget
{
    public int Id { get; set; }

    public int HouseholdId { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MonthlyLimit { get; set; }
}
