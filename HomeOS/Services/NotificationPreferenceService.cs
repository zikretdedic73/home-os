using HomeOS.Data;
using HomeOS.Models.Notifications;
using Microsoft.EntityFrameworkCore;

namespace HomeOS.Services;

public class NotificationPreferenceService : INotificationPreferenceService
{
    private readonly ApplicationDbContext _context;

    public NotificationPreferenceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsEnabledAsync(int memberId, NotificationCategory category)
    {
        var pref = await _context.MemberNotificationPreferences
            .FirstOrDefaultAsync(p => p.MemberId == memberId && p.Category == category);

        // No row => never changed => enabled (opt-out model).
        return pref?.IsEnabled ?? true;
    }

    public async Task<IReadOnlyDictionary<NotificationCategory, bool>> GetSettingsAsync(int memberId)
    {
        var stored = await _context.MemberNotificationPreferences
            .Where(p => p.MemberId == memberId)
            .ToDictionaryAsync(p => p.Category, p => p.IsEnabled);

        return Enum.GetValues<NotificationCategory>()
            .ToDictionary(c => c, c => stored.TryGetValue(c, out var v) ? v : true);
    }

    public async Task SetAsync(int memberId, NotificationCategory category, bool isEnabled)
    {
        var pref = await _context.MemberNotificationPreferences
            .FirstOrDefaultAsync(p => p.MemberId == memberId && p.Category == category);

        if (pref == null)
        {
            pref = new MemberNotificationPreference { MemberId = memberId, Category = category };
            _context.MemberNotificationPreferences.Add(pref);
        }

        pref.IsEnabled = isEnabled;
        await _context.SaveChangesAsync();
    }
}
