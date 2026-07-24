namespace HomeOS.Models.Shopping;

// A single line on a shopping list. Quantity is a free-text string (e.g.
// "2kg", "1 pc") - no structured unit needed for this scope
// (Docs/04_Model_Podataka.md, section 10).
public class ShoppingListItem
{
    public int Id { get; set; }
    public int ShoppingListId { get; set; }
    public ShoppingList? ShoppingList { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Quantity { get; set; }
    public bool IsChecked { get; set; }
    public int? AddedByMemberId { get; set; }
}
