using HomeOS.Models.Tasks;

namespace HomeOS.Models.Notes;

// Join table reusing the SHARED Tag entity (household-level, not owned by one
// module) - Docs/04_Model_Podataka.md, section 7. Same Tag rows can label
// both tasks and notes.
public class NoteTag
{
    public int NoteId { get; set; }
    public Note? Note { get; set; }
    public int TagId { get; set; }
    public Tag? Tag { get; set; }
}
