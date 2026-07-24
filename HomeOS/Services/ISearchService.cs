namespace HomeOS.Services;

// System-level universal search (Docs/00_Specifikacija_Izvor.md, "Univerzalna
// pretraga"). Aggregates every registered ISearchable and drops results from
// modules that are currently disabled for the household.
public interface ISearchService
{
    Task<List<SearchResult>> SearchAsync(int householdId, string query);
}
