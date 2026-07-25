using HomeOS.Models.Households;

namespace HomeOS.Models.Notifications;

// One row per (member, category) that a member has explicitly turned OFF.
// Absence of a row means "enabled" - so notifications are opt-out and a brand
// new member gets everything until they change their settings. This keeps the
// table small and avoids seeding a row per member per category.
public class MemberNotificationPreference
{
    public int Id { get; set; }

    public int MemberId { get; set; }
    public Member? Member { get; set; }

    public NotificationCategory Category { get; set; }

    // Stored explicitly (rather than relying on row presence alone) so the
    // settings screen can round-trip the value and future defaults can change.
    public bool IsEnabled { get; set; } = true;
}
