namespace HomeOS.Services;

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
}
