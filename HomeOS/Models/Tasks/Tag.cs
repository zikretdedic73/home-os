namespace HomeOS.Models.Tasks;

// Note: Tag is shared at the household level (not owned by Tasks alone) -
// the Notes module will reuse this same entity later (see Docs/04_Model_Podataka.md).
public class Tag
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public string Name { get; set; } = string.Empty;
}
