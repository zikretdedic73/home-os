namespace HomeOS.Services;

// One line inside a dashboard widget.
public record DashboardItem(string Title, string? Url, string? Badge, bool Highlight);

// One module's section on the "Today" dashboard.
public record DashboardWidget(string ModuleKey, int SortOrder, string Title, string EmptyText, List<DashboardItem> Items);

// A module contributes its own dashboard section by registering this in DI -
// the dashboard is generated from the registered contributors (filtered to
// enabled modules), so a new module automatically appears on the "command
// center" too (Docs/00_Specifikacija_Izvor.md, "automatski je vidljiva na
// komandnoj tabli"). ModuleKey ties the widget to its module's enabled state.
public interface IDashboardContributor
{
    string ModuleKey { get; }
    int SortOrder { get; }
    Task<DashboardWidget> BuildAsync(int householdId, int memberId);
}
