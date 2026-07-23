namespace HomeOS.Models.Tasks;

// Named "TaskState", not "TaskStatus", to avoid a name collision with the
// built-in System.Threading.Tasks.TaskStatus enum.
public enum TaskState
{
    Open,
    InProgress,
    Done
}
