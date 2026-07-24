namespace HomeOS.Models.Modules;

// Per-household grant/revoke state for a permission a module requested
// (Docs/00_Specifikacija_Izvor.md, "Kontrola i privatnost domaćinstva").
// Absence of a row means granted by default for built-in modules; a row with
// IsGranted=false means the household explicitly revoked it.
public class ModulePermissionState
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public string Permission { get; set; } = string.Empty;
    public bool IsGranted { get; set; } = true;
}
