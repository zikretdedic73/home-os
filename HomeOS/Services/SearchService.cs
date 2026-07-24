namespace HomeOS.Services;

public class SearchService : ISearchService
{
    private readonly IEnumerable<ISearchable> _providers;
    private readonly IModuleRegistry _registry;

    public SearchService(IEnumerable<ISearchable> providers, IModuleRegistry registry)
    {
        _providers = providers;
        _registry = registry;
    }

    public async Task<List<SearchResult>> SearchAsync(int householdId, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<SearchResult>();

        // Only include results from modules the household has enabled - a
        // disabled module must disappear from search too, not just navigation.
        var enabledKeys = (await _registry.GetEnabledAsync(householdId))
            .Select(m => m.Key)
            .ToHashSet();

        var results = new List<SearchResult>();
        foreach (var provider in _providers)
        {
            var providerResults = await provider.SearchAsync(householdId, query.Trim());
            results.AddRange(providerResults.Where(r => enabledKeys.Contains(r.ModuleName)));
        }

        return results;
    }
}
