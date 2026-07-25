namespace HomeOS.Models.Common;

// The kind of entity an ItemShare points at. A single share table serves every
// module (one row = "this item, of this type, is shared with this member"),
// so adding a new shareable module means adding a value here rather than a new
// per-module share table.
public enum ShareableType
{
    Task = 0,
    Reminder = 1,
    Note = 2,
    Event = 3,
    ShoppingList = 4,
    Transaction = 5,
    Bill = 6,
    Document = 7
}

// A grant that makes one item (identified by Type + ItemId) visible to one
// member, used when an item's Visibility is SpecificMembers. Kept as a thin,
// polymorphic join rather than a navigation on each entity so the visibility
// rule stays uniform across modules (Docs/04_Model_Podataka.md).
public class ItemShare
{
    public int Id { get; set; }
    public ShareableType Type { get; set; }
    public int ItemId { get; set; }
    public int MemberId { get; set; }
}
