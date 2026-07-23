namespace HomeOS.Models.Tasks;

public class TaskTag
{
    public int TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public int TagId { get; set; }
    public Tag? Tag { get; set; }
}
