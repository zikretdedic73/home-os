using HomeOS.Models.Tasks;

namespace HomeOS.Models.Kanban;

// A card is a link to an existing TaskItem, never a copy of its data
// (Docs/04_Model_Podataka.md, section 4 - "gradi na postojećem"). Moving a
// card between columns updates Card.ColumnId and TaskItem.Status together.
public class Card
{
    public int Id { get; set; }
    public int ColumnId { get; set; }
    public Column? Column { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public int SortOrder { get; set; }
}
