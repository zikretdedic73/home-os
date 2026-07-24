namespace HomeOS.Models.Modules;

// Per-household enable/disable state for a module ("instalacija/uklanjanje...
// reverzibilni" - Docs/00_Specifikacija_Izvor.md). Absence of a row means the
// module is enabled by default; a row with IsEnabled=false hides it cleanly
// from navigation, search and the dashboard without deleting any data.
public class ModuleState
{
    public int Id { get; set; }
    public int HouseholdId { get; set; }
    public string ModuleKey { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
}
