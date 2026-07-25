using HomeOS.Data;
using HomeOS.Models.Notifications;
using HomeOS.Models.Reminders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Services;

public class ReminderNotificationService : IReminderNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly INotificationPreferenceService _preferences;
    private readonly IAppUrlBuilder _urlBuilder;
    private readonly IStringLocalizer<ReminderNotificationService> _localizer;

    public ReminderNotificationService(
        ApplicationDbContext context,
        IEmailSender emailSender,
        UserManager<IdentityUser> userManager,
        INotificationPreferenceService preferences,
        IAppUrlBuilder urlBuilder,
        IStringLocalizer<ReminderNotificationService> localizer)
    {
        _context = context;
        _emailSender = emailSender;
        _userManager = userManager;
        _preferences = preferences;
        _urlBuilder = urlBuilder;
        _localizer = localizer;
    }

    public async Task ProcessDueRemindersAsync(int householdId)
    {
        var nowUtc = DateTime.UtcNow;

        var candidates = await _context.Reminders
            .Where(r => r.HouseholdId == householdId && !r.IsDeleted && !r.IsResolved && r.TriggerAtUtc <= nowUtc)
            .Include(r => r.Recipients)
            .ToListAsync();

        var dueReminders = candidates.Where(r => r.IsDue(nowUtc)).ToList();
        var anyNotified = false;

        foreach (var reminder in dueReminders)
        {
            foreach (var recipient in reminder.Recipients.Where(rec => !rec.NotifiedViaEmail))
            {
                var member = await _context.Members.FindAsync(recipient.MemberId);
                var user = member == null ? null : await _userManager.FindByIdAsync(member.IdentityUserId);

                var emailSent = false;
                // Respect the recipient's per-category setting (Docs/00 -
                // "uključivanje/isključivanje kategorija obavještenja").
                var wantsEmail = await _preferences.IsEnabledAsync(recipient.MemberId, NotificationCategory.ReminderDue);
                if (user?.Email != null && wantsEmail)
                {
                    var subject = string.Format(_localizer["ReminderDueEmailSubject"], reminder.Title);
                    var body = string.Format(
                        _localizer["ReminderDueEmailBody"],
                        reminder.Title,
                        reminder.TriggerAtUtc.ToString("dd.MM.yyyy HH:mm"));

                    // One-click link back to the reminder.
                    var url = _urlBuilder.ActionUrl("Edit", "Reminders", new { id = reminder.Id });
                    if (!string.IsNullOrEmpty(url))
                        body += string.Format(_localizer["ReminderDueEmailLink"], url);

                    emailSent = await _emailSender.SendEmailAsync(user.Email, subject, body);
                }

                // Only mark as notified once the email actually went out - a
                // failed send (e.g. Resend sandbox restrictions, no API key
                // yet) should retry on the next due-reminder check rather
                // than being silently marked as delivered.
                recipient.NotifiedViaEmail = emailSent;
                recipient.NotifiedInAppAtUtc = nowUtc;
                anyNotified = true;
            }
        }

        if (anyNotified)
            await _context.SaveChangesAsync();
    }

    public async Task<List<Reminder>> GetActiveRemindersForMemberAsync(int householdId, int memberId)
    {
        var nowUtc = DateTime.UtcNow;

        return await _context.Reminders
            .Where(r => r.HouseholdId == householdId && !r.IsDeleted && !r.IsResolved
                && r.Recipients.Any(rec => rec.MemberId == memberId)
                && r.TriggerAtUtc <= nowUtc
                && (r.SnoozedUntilUtc == null || r.SnoozedUntilUtc <= nowUtc))
            .OrderBy(r => r.TriggerAtUtc)
            .ToListAsync();
    }
}
