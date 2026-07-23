namespace HomeOS.Models.Common;

// Common base for every entity that represents user content.
// See Docs/04_Model_Podataka.md, section 1.
public abstract class BaseEntity
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public int OwnerId { get; set; }
    public Visibility Visibility { get; set; } = Visibility.Household;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; } = false;
}
