namespace HomeOS.Services;

// One result row a module contributes to a search query. ModuleName is a
// stable identifier (not user-facing text) - the search UI (Day 4) maps it
// to a localized label, the same way enum values are mapped elsewhere.
public record SearchResult(string ModuleName, string Title, string? Snippet, string Url);

// Core capability every searchable module implements and registers itself
// for (see Docs/00_Specifikacija_Izvor.md, "Nova aplikacija ... automatski
// je vidljiva ... u pretrazi" - the Shell/SearchService never needs to know
// which modules exist ahead of time, it just aggregates every registered
// ISearchable). See Docs/02_Pravila_Programiranja.md, section 1.2.
public interface ISearchable
{
    Task<List<SearchResult>> SearchAsync(int householdId, string query);
}
