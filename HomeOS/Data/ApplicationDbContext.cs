using HomeOS.Models.Calendar;
using HomeOS.Models.Households;
using HomeOS.Models.Modules;
using HomeOS.Models.Notes;
using HomeOS.Models.Notifications;
using HomeOS.Models.Reminders;
using HomeOS.Models.Shopping;
using HomeOS.Models.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Data;

// Inherits IdentityDbContext so Identity tables (AspNetUsers, etc.) and our
// own tables live in the same database/context - standard ASP.NET Core approach.
public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Household> Households => Set<Household>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<SubTask> SubTasks => Set<SubTask>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TaskTag> TaskTags => Set<TaskTag>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<ReminderRecipient> ReminderRecipients => Set<ReminderRecipient>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventAttendee> EventAttendees => Set<EventAttendee>();
    public DbSet<ModuleState> ModuleStates => Set<ModuleState>();
    public DbSet<ModulePermissionState> ModulePermissionStates => Set<ModulePermissionState>();
    public DbSet<MemberModuleAccess> MemberModuleAccesses => Set<MemberModuleAccess>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<NoteTag> NoteTags => Set<NoteTag>();
    public DbSet<ShoppingList> ShoppingLists => Set<ShoppingList>();
    public DbSet<ShoppingListItem> ShoppingListItems => Set<ShoppingListItem>();
    public DbSet<MemberNotificationPreference> MemberNotificationPreferences => Set<MemberNotificationPreference>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // TaskTag is a many-to-many join table with a composite key
        builder.Entity<TaskTag>().HasKey(tt => new { tt.TaskItemId, tt.TagId });

        builder.Entity<TaskTag>()
            .HasOne(tt => tt.TaskItem)
            .WithMany(t => t.TaskTags)
            .HasForeignKey(tt => tt.TaskItemId);

        builder.Entity<TaskTag>()
            .HasOne(tt => tt.Tag)
            .WithMany()
            .HasForeignKey(tt => tt.TagId);

        builder.Entity<SubTask>()
            .HasOne(s => s.TaskItem)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(s => s.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // ReminderRecipient is a many-to-many join table with a composite key
        builder.Entity<ReminderRecipient>().HasKey(rr => new { rr.ReminderId, rr.MemberId });

        builder.Entity<ReminderRecipient>()
            .HasOne(rr => rr.Reminder)
            .WithMany(r => r.Recipients)
            .HasForeignKey(rr => rr.ReminderId)
            .OnDelete(DeleteBehavior.Cascade);

        // EventAttendee is a many-to-many join table with a composite key
        builder.Entity<EventAttendee>().HasKey(ea => new { ea.EventId, ea.MemberId });

        builder.Entity<EventAttendee>()
            .HasOne(ea => ea.Event)
            .WithMany(e => e.Attendees)
            .HasForeignKey(ea => ea.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        // HouseholdId indexes - rule from Docs/02_Pravila_Programiranja.md, section 3
        builder.Entity<TaskItem>().HasIndex(t => t.HouseholdId);
        builder.Entity<Member>().HasIndex(m => m.HouseholdId);
        builder.Entity<Tag>().HasIndex(t => t.HouseholdId);
        builder.Entity<Reminder>().HasIndex(r => r.HouseholdId);
        builder.Entity<Event>().HasIndex(e => e.HouseholdId);

        // One state row per (household, module); look-ups filter by household.
        builder.Entity<ModuleState>().HasIndex(s => new { s.HouseholdId, s.ModuleKey }).IsUnique();
        builder.Entity<ModulePermissionState>()
            .HasIndex(p => new { p.HouseholdId, p.ModuleKey, p.Permission }).IsUnique();
        builder.Entity<MemberModuleAccess>()
            .HasIndex(a => new { a.HouseholdId, a.MemberId, a.ModuleKey }).IsUnique();

        // NoteTag is a many-to-many join reusing the shared Tag entity
        builder.Entity<NoteTag>().HasKey(nt => new { nt.NoteId, nt.TagId });
        builder.Entity<NoteTag>()
            .HasOne(nt => nt.Note).WithMany(n => n.NoteTags)
            .HasForeignKey(nt => nt.NoteId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<NoteTag>()
            .HasOne(nt => nt.Tag).WithMany()
            .HasForeignKey(nt => nt.TagId);

        builder.Entity<Note>().HasIndex(n => n.HouseholdId);

        builder.Entity<ShoppingListItem>()
            .HasOne(i => i.ShoppingList).WithMany(l => l.Items)
            .HasForeignKey(i => i.ShoppingListId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ShoppingList>().HasIndex(l => l.HouseholdId);

        // One preference row per (member, category).
        builder.Entity<MemberNotificationPreference>()
            .HasIndex(p => new { p.MemberId, p.Category }).IsUnique();
    }
}
