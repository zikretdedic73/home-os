using HomeOS.Services;

namespace HomeOS.Models.Search;

public class SearchResultsViewModel
{
    public string Query { get; set; } = string.Empty;
    public List<SearchResultGroup> Groups { get; set; } = new();
    public bool HasResults => Groups.Any();
}

// Results for one module, under its localized display name.
public record SearchResultGroup(string ModuleDisplayName, List<SearchResult> Results);
