using System.Diagnostics;
using HomeOS.Data;
using HomeOS.Models;
using HomeOS.Models.Events;
using HomeOS.Models.Home;
using HomeOS.Models.Reminders;
using HomeOS.Models.Tasks;
using HomeOS.Services;
using HomeOS.Services.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace HomeOS.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentHouseholdService _household;
        private readonly IReminderNotificationService _reminders;
        private readonly IStringLocalizer<HomeController> _localizer;
        private readonly IEventBus _eventBus;
        private readonly IModuleRegistry _moduleRegistry;
        private readonly IEnumerable<IDashboardContributor> _dashboardContributors;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            ICurrentHouseholdService household,
            IReminderNotificationService reminders,
            IStringLocalizer<HomeController> localizer,
            IEventBus eventBus,
            IModuleRegistry moduleRegistry,
            IEnumerable<IDashboardContributor> dashboardContributors)
        {
            _logger = logger;
            _context = context;
            _household = household;
            _reminders = reminders;
            _localizer = localizer;
            _eventBus = eventBus;
            _moduleRegistry = moduleRegistry;
            _dashboardContributors = dashboardContributors;
        }

        // "Today" dashboard - generated from every enabled module's dashboard
        // contributor, so a new module automatically gets a section and a
        // disabled one drops off (Docs/00_Specifikacija_Izvor.md, "automatski
        // vidljiva na komandnoj tabli").
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var householdId = await _household.GetCurrentHouseholdIdAsync();
            var memberId = await _household.GetCurrentMemberIdAsync();

            // Checking for due reminders on Dashboard load is an accepted
            // simplification for this scope - see Docs/01_Roadmap.md, section 2.2.
            await _reminders.ProcessDueRemindersAsync(householdId);

            var enabledKeys = (await _moduleRegistry.GetVisibleForMemberAsync(householdId, memberId))
                .Select(m => m.Key)
                .ToHashSet();

            var widgets = new List<DashboardWidget>();
            foreach (var contributor in _dashboardContributors.Where(c => enabledKeys.Contains(c.ModuleKey)))
            {
                widgets.Add(await contributor.BuildAsync(householdId, memberId));
            }

            var viewModel = new DashboardViewModel
            {
                Widgets = widgets.OrderBy(w => w.SortOrder).ToList()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Shell-owned language switch (Docs/02_Pravila_Programiranja.md, section 5.5) -
        // stores the choice in the standard culture cookie and redirects back.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

            return LocalRedirect(returnUrl);
        }

        // Quick Capture - available from every screen via the navbar (Docs/01_Roadmap.md,
        // section 2.1). Only Tasks and Reminders exist so far; Notes is added
        // once that module exists (Day 3), without changing this action's shape.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCapture(string type, string title, DateTime? when, string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                var householdId = await _household.GetCurrentHouseholdIdAsync();
                var memberId = await _household.GetCurrentMemberIdAsync();

                if (type == "reminder")
                {
                    var reminder = new Reminder
                    {
                        HouseholdId = householdId,
                        OwnerId = memberId,
                        Title = title.Trim(),
                        TriggerAtUtc = when ?? DateTime.UtcNow,
                        SourceType = ReminderSourceType.Manual
                    };
                    _context.Reminders.Add(reminder);
                    await _context.SaveChangesAsync();
                    _context.ReminderRecipients.Add(new ReminderRecipient { ReminderId = reminder.Id, MemberId = memberId });
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var task = new TaskItem
                    {
                        HouseholdId = householdId,
                        OwnerId = memberId,
                        Title = title.Trim(),
                        DueDate = when
                    };
                    _context.Tasks.Add(task);
                    await _context.SaveChangesAsync();

                    // Same "key moment" as Tasks/Create - Reminders reacts.
                    if (task.DueDate.HasValue)
                    {
                        await _eventBus.PublishAsync(new TaskWithDueDateCreatedEvent(
                            householdId, task.Id, memberId, task.Title, task.DueDate.Value));
                    }
                }

                TempData["Success"] = _localizer["QuickCaptureSuccessMessage"].Value;
            }

            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? Url.Action(nameof(Index))! : returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
