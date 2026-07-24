using HomeOS.Services;

namespace HomeOS.Models.Home;

// The dashboard is a list of widgets contributed by enabled modules - the
// Shell no longer hardcodes which module sections exist (see
// Docs/00_Specifikacija_Izvor.md, "automatski vidljiva na komandnoj tabli").
public class DashboardViewModel
{
    public List<DashboardWidget> Widgets { get; set; } = new();
}
