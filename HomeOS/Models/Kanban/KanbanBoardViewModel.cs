using HomeOS.Models.Tasks;

namespace HomeOS.Models.Kanban;

// View models for the single auto-formed Kanban board (no persisted board/
// column/card entities - the board is a projection of tasks by status).
public class KanbanBoardViewModel
{
    public List<KanbanColumnViewModel> Columns { get; set; } = new();
}

public record KanbanColumnViewModel(TaskState Status, List<TaskItem> Tasks);
