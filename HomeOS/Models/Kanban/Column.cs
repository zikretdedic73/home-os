using HomeOS.Models.Tasks;

namespace HomeOS.Models.Kanban;

// A column on a board. Each column maps to a task status - moving a card into
// the column sets the linked task's status to MappedStatus (see
// Docs/04_Model_Podataka.md, section 4). Columns are auto-seeded per board
// (To do / In progress / Done); custom columns are a possible V2.
public class Column
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public Board? Board { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public TaskState MappedStatus { get; set; }

    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
