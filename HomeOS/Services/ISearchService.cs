namespace HomeOS.Services;

// System-level universal search (Docs/00_Specifikacija_Izvor.md, "Univerzalna
// pretraga"). Aggregates every registered ISearchable and drops results from
// modules that are currently disabled for the household.
public interface ISearchService
{
    // memberId is used so results from modules the member can't access (RBAC)
    // are excluded, alongside modules disabled for the household.
    Task<List<SearchResult>> SearchAsync(int householdId, int memberId, string query);
}
