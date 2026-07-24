using System.ComponentModel.DataAnnotations;
using HomeOS.Models.Common;

namespace HomeOS.Models.Notes;

public class Note : BaseEntity
{
    // Nullable - a journal entry can use its date instead of a title.
    [MaxLength(200)]
    public string? Title { get; set; }

    [Required]
    public string Content { get; set; } = string.Empty;

    public bool IsJournalEntry { get; set; }

    // Set only for journal entries - one per (member, date).
    public DateOnly? JournalDate { get; set; }

    // Optional link into the existing data network (a task or an event),
    // without duplicating it - same idea as Reminder.SourceType/SourceId.
    public NoteLinkType LinkType { get; set; } = NoteLinkType.None;
    public int? LinkedEntityId { get; set; }

    public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
}
