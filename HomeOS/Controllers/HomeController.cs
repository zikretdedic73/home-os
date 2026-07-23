using System.Diagnostics;
using HomeOS.Data;
using HomeOS.Models;
using HomeOS.Models.Home;
using HomeOS.Models.Reminders;
using HomeOS.Models.Tasks;
using HomeOS.Services;
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

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            ICurrentHouseholdService household,
            IReminderNotificationService reminders,
            IStringLocalizer<HomeController> localizer)
        {
            _logger = logger;
            _context = context;
            _household = household;
            _reminders = reminders;
            _localizer = localizer;
        }

        // "Today" dashboard - aggregates Tasks, Calendar and Reminders (the
        // modules that exist so far). Finance/Life Admin sections are added
        // once those modules exist (Docs/01_Roadmap.md, Day 4).
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var householdId = await _household.GetCurrentHouseholdIdAsync();
            var memberId = await _household.GetCurrentMemberIdAsync();

            // Checking for due reminders on Dashboard load is an accepted
            // simplification for this scope - see Docs/01_Roadmap.md, section 2.2.
            await _reminders.ProcessDueRemindersAsync(householdId);

            var todayUtc = DateTime.UtcNow.Date;
            var tomorrowUtc = todayUtc.AddDays(1);

            var dueOrOverdueTasks = await _context.Tasks
                .Where(t => t.HouseholdId == householdId && !t.IsDeleted
                    && t.Status != TaskState.Done
                    && t.DueDate != null && t.DueDate < tomorrowUtc)
                .OrderBy(t => t.DueDate)
                .ToListAsync();

            var todayEvents = await _context.Events
                .Where(e => e.HouseholdId == householdId && !e.IsDeleted
                    && e.StartsAtUtc < tomorrowUtc && e.EndsAtUtc >= todayUtc)
                .OrderBy(e => e.StartsAtUtc)
                .ToListAsync();

            var activeReminders = await _reminders.GetActiveRemindersForMemberAsync(householdId, memberId);

            var viewModel = new DashboardViewModel
            {
                DueOrOverdueTasks = dueOrOverdueTasks,
                TodayEvents = todayEvents,
                ActiveReminders = activeReminders
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
                }
                else
                {
                    _context.Tasks.Add(new TaskItem
                    {
                        HouseholdId = householdId,
                        OwnerId = memberId,
                        Title = title.Trim(),
                        DueDate = when
                    });
                }

                await _context.SaveChangesAsync();
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
