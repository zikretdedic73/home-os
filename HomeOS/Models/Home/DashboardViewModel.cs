using HomeOS.Models.Calendar;
using HomeOS.Models.Reminders;
using HomeOS.Models.Tasks;

namespace HomeOS.Models.Home;

// Aggregates across modules for the "Today" screen - the Shell reads other
// modules' data directly (see Docs/00_Specifikacija_Izvor.md, "Gradite na
// postojećem"). Finance/Life Admin sections are added once those modules
// exist (Docs/01_Roadmap.md, Day 4).
public class DashboardViewModel
{
    public List<TaskItem> DueOrOverdueTasks { get; set; } = new();
    public List<Event> TodayEvents { get; set; } = new();
    public List<Reminder> ActiveReminders { get; set; } = new();
}
