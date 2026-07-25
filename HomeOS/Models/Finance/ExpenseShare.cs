using System.ComponentModel.DataAnnotations.Schema;
using HomeOS.Models.Households;

namespace HomeOS.Models.Finance;

// One member's portion of a split expense. The sum of a transaction's shares
// equals the transaction Amount. Kept as a simple join (no visibility of its
// own - it inherits the parent transaction's).
public class ExpenseShare
{
    public int Id { get; set; }

    public int TransactionId { get; set; }
    public Transaction? Transaction { get; set; }

    public int MemberId { get; set; }
    public Member? Member { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
}
