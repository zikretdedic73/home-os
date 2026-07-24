using HomeOS.Models.Search;
using HomeOS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly ISearchService _search;
    private readonly IModuleRegistry _registry;
    private readonly ICurrentHouseholdService _household;

    public SearchController(ISearchService search, IModuleRegistry registry, ICurrentHouseholdService household)
    {
        _search = search;
        _registry = registry;
        _household = household;
    }

    // GET: /Search?q=...
    public async Task<IActionResult> Index(string? q)
    {
        var householdId = await _household.GetCurrentHouseholdIdAsync();
        var memberId = await _household.GetCurrentMemberIdAsync();
        var results = await _search.SearchAsync(householdId, memberId, q ?? string.Empty);

        // Map each module key to its localized display name (owned by the
        // module), so results can be grouped under a readable heading.
        var moduleNames = (await _registry.GetVisibleForMemberAsync(householdId, memberId))
            .ToDictionary(m => m.Key, m => m.DisplayName);

        var viewModel = new SearchResultsViewModel
        {
            Query = q ?? string.Empty,
            Groups = results
                .GroupBy(r => r.ModuleName)
                .Select(g => new SearchResultGroup(
                    moduleNames.TryGetValue(g.Key, out var name) ? name : g.Key,
                    g.ToList()))
                .OrderBy(g => g.ModuleDisplayName)
                .ToList()
        };

        return View(viewModel);
    }
}
