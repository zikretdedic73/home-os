using HomeOS.Models.Notifications;

namespace HomeOS.Services;

// Central gate every notification path asks before sending (email or in-app),
// so "individualna podešavanja obavještenja" is enforced in one place rather
// than re-checked per module (Docs/02 - shared core service).
public interface INotificationPreferenceService
{
    // True unless the member has explicitly turned this category off. New
    // members and untouched categories default to enabled (opt-out model).
    Task<bool> IsEnabledAsync(int memberId, NotificationCategory category);

    // The full on/off map for a member, with defaults filled in - used by the
    // settings screen.
    Task<IReadOnlyDictionary<NotificationCategory, bool>> GetSettingsAsync(int memberId);

    // Persists the member's choice for a single category.
    Task SetAsync(int memberId, NotificationCategory category, bool isEnabled);
}
