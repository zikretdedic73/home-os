using HomeOS.Data;
using HomeOS.Models.Events;
using HomeOS.Models.Notifications;
using HomeOS.Services;
using HomeOS.Services.Events;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace HomeOS.Models.Tasks;

// Emails the assignee when a task is assigned to them. Subscribes to
// TaskAssignedEvent on the shared event bus, so Tasks stays decoupled from the
// email integration (Docs/02 - one email integration, reused; notifications
// react to "key moments" rather than being called inline). Honors the member's
// per-category notification settings before sending.
public class TaskAssignedEmailHandler : IEventHandler<TaskAssignedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly INotificationPreferenceService _preferences;
    private readonly IAppUrlBuilder _urlBuilder;
    private readonly IStringLocalizer<TaskAssignedEmailHandler> _localizer;

    public TaskAssignedEmailHandler(
        ApplicationDbContext context,
        IEmailSender emailSender,
        UserManager<IdentityUser> userManager,
        INotificationPreferenceService preferences,
        IAppUrlBuilder urlBuilder,
        IStringLocalizer<TaskAssignedEmailHandler> localizer)
    {
        _context = context;
        _emailSender = emailSender;
        _userManager = userManager;
        _preferences = preferences;
        _urlBuilder = urlBuilder;
        _localizer = localizer;
    }

    public async Task HandleAsync(TaskAssignedEvent integrationEvent)
    {
        // Respect the assignee's notification settings - they may have turned
        // the "assigned task" category off (Docs/00 - individualna podešavanja).
        if (!await _preferences.IsEnabledAsync(integrationEvent.AssigneeMemberId, NotificationCategory.TaskAssigned))
            return;

        var member = await _context.Members.FindAsync(integrationEvent.AssigneeMemberId);
        if (member == null) return;

        var user = await _userManager.FindByIdAsync(member.IdentityUserId);
        if (user?.Email == null) return;

        var subject = string.Format(_localizer["TaskAssignedEmailSubject"], integrationEvent.Title);
        var due = integrationEvent.DueDateUtc.HasValue
            ? integrationEvent.DueDateUtc.Value.ToString("dd.MM.yyyy")
            : _localizer["NoDueDate"].Value;

        var body = string.Format(_localizer["TaskAssignedEmailBody"], integrationEvent.Title, due);

        // Deep link straight to the task, so the recipient opens it in one click.
        var url = _urlBuilder.ActionUrl("Edit", "Tasks", new { id = integrationEvent.TaskId });
        if (!string.IsNullOrEmpty(url))
            body += string.Format(_localizer["TaskAssignedEmailLink"], url);

        await _emailSender.SendEmailAsync(user.Email, subject, body);
    }
}
