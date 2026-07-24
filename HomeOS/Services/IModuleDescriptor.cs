namespace HomeOS.Services;

// A data-access permission a module requests from the household (e.g. Calendar
// requesting to read Tasks). Key is stable; DisplayName is localized by the
// requesting module. See Docs/00_Specifikacija_Izvor.md, "Kontrola i
// privatnost domaćinstva".
public record ModulePermission(string Key, string DisplayName);

// Every installable module describes itself through this contract and
// registers itself in DI - the Shell never hardcodes a list of modules
// (see Docs/00_Specifikacija_Izvor.md, "Nove aplikacije su ravnopravni
// građani": automatski vidljiva u navigaciji/pretrazi, bez izmjena na
// postojećim modulima). Navigation, search and the module manager are all
// generated from the set of registered descriptors.
public interface IModuleDescriptor
{
    // Stable identifier, not user-facing (e.g. "Tasks"). Matches the
    // SearchResult.ModuleName a module's ISearchable produces, and the
    // ModuleState.ModuleKey persisted when a module is enabled/disabled.
    string Key { get; }

    // MVC controller the nav link points at (e.g. "Tasks" -> /Tasks).
    string Controller { get; }

    // Emoji shown next to the nav label.
    string Icon { get; }

    // Left-to-right order in the navbar.
    int SortOrder { get; }

    // Localized nav label - resolved through the module's OWN localizer, so
    // the Shell never holds module display text (Docs/02_Pravila_Programiranja.md,
    // section 5).
    string DisplayName { get; }

    // Cross-module data-access permissions this module requests. Most modules
    // only use their own data and request none; the household reviews and can
    // revoke any granted permission (Docs/00_Specifikacija_Izvor.md, section
    // "Kontrola i privatnost domaćinstva").
    IReadOnlyList<ModulePermission> RequestedPermissions => Array.Empty<ModulePermission>();
}
