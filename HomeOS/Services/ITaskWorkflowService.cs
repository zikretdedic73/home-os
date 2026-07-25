using HomeOS.Models.Tasks;

namespace HomeOS.Services;

// Shared task-status logic used by both TasksController and the Kanban board,
// so the "recurring task spawns its next instance on completion" rule lives in
// one place (Docs/00_Specifikacija_Izvor.md - "ponavljajući zadaci").
public interface ITaskWorkflowService
{
    // Applies a status change to the task. If it transitions INTO Done and has
    // a recurrence rule, creates the next occurrence (linked via ParentTaskId)
    // and returns it; otherwise returns null. Does not call SaveChanges for the
    // status change itself - the caller owns that - but persists the spawned task.
    Task<TaskItem?> ApplyStatusChangeAsync(TaskItem task, TaskState newStatus);
}
