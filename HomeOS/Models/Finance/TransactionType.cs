namespace HomeOS.Models.Finance;

// Whether a transaction adds to or subtracts from the household's money.
// Budgets and the monthly summary only track Expense; Income is shown for a
// complete picture.
public enum TransactionType
{
    Expense = 0,
    Income = 1
}
