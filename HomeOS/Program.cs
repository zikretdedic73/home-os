using HomeOS.Data;
using HomeOS.Models.Calendar;
using HomeOS.Models.Events;
using HomeOS.Models.Finance;
using HomeOS.Models.Kanban;
using HomeOS.Models.LifeAdmin;
using HomeOS.Models.Notes;
using HomeOS.Models.Reminders;
using HomeOS.Models.Shopping;
using HomeOS.Models.Tasks;
using HomeOS.Services;
using HomeOS.Services.Events;
using HomeOS.Services.Realtime;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// --- Database (EF Core + SQL Server / LocalDB) ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Identity (authentication) - default UI is built in, no scaffolding needed ---
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// --- Core services (shared by every module - see Docs/02_Pravila_Programiranja.md) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentHouseholdService, CurrentHouseholdService>();
builder.Services.AddSingleton<IRecurrenceService, RecurrenceService>();
// E-mail is sent via Gmail SMTP (MailKit). The App Password is a secret and
// comes from user-secrets/environment (see README/appsettings placeholder).
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSender, GmailEmailSender>();
builder.Services.AddScoped<IReminderNotificationService, ReminderNotificationService>();
builder.Services.AddScoped<ITaskWorkflowService, TaskWorkflowService>();
builder.Services.AddScoped<INotificationPreferenceService, NotificationPreferenceService>();
builder.Services.AddScoped<IItemSharingService, ItemSharingService>();
builder.Services.AddScoped<IAppUrlBuilder, AppUrlBuilder>();

// --- Event bus - modules publish "key moments" and others react, without
// direct dependency (Docs/00_Specifikacija_Izvor.md, "Kooperacija bez direktne
// zavisnosti"). Each subscription is one AddScoped<IEventHandler<T>, ...> line. ---
builder.Services.AddScoped<IEventBus, InProcessEventBus>();
builder.Services.AddScoped<IEventHandler<TaskWithDueDateCreatedEvent>, TaskWithDueDateCreatedHandler>();
builder.Services.AddScoped<IEventHandler<TaskAssignedEvent>, TaskAssignedEmailHandler>();
builder.Services.AddScoped<IEventHandler<BillDueDateCreatedEvent>, BillDueReminderHandler>();
builder.Services.AddScoped<IEventHandler<DocumentExpiryCreatedEvent>, DocumentExpiryReminderHandler>();

// --- Module registry - each module registers a descriptor (nav/search/module
// manager are generated from these, never hardcoded) and its ISearchable
// provider. Adding a module = adding its lines here, with no change to the
// Shell (Docs/00_Specifikacija_Izvor.md, "Nove aplikacije su ravnopravni
// građani"; Docs/02_Pravila_Programiranja.md, sections 1.2 & 1.4). ---
builder.Services.AddScoped<IModuleRegistry, ModuleRegistry>();
builder.Services.AddScoped<ISearchService, SearchService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IMemberAccessService, MemberAccessService>();

builder.Services.AddScoped<IModuleDescriptor, TasksModule>();
builder.Services.AddScoped<ISearchable, TaskSearchProvider>();
builder.Services.AddScoped<IDashboardContributor, TasksDashboardContributor>();

builder.Services.AddScoped<IModuleDescriptor, CalendarModule>();
builder.Services.AddScoped<ISearchable, EventSearchProvider>();
builder.Services.AddScoped<IDashboardContributor, CalendarDashboardContributor>();

builder.Services.AddScoped<IModuleDescriptor, RemindersModule>();
builder.Services.AddScoped<ISearchable, ReminderSearchProvider>();
builder.Services.AddScoped<IDashboardContributor, RemindersDashboardContributor>();

// Kanban is a live view over Tasks (auto-formed by status), so it has no data
// of its own to search - tasks are already covered by TaskSearchProvider.
builder.Services.AddScoped<IModuleDescriptor, KanbanModule>();

builder.Services.AddScoped<IModuleDescriptor, NotesModule>();
builder.Services.AddScoped<ISearchable, NoteSearchProvider>();
builder.Services.AddScoped<IDashboardContributor, NotesDashboardContributor>();

builder.Services.AddScoped<IModuleDescriptor, ShoppingListsModule>();
builder.Services.AddScoped<ISearchable, ShoppingListSearchProvider>();

builder.Services.AddScoped<IModuleDescriptor, FinanceModule>();
builder.Services.AddScoped<ISearchable, FinanceSearchProvider>();
builder.Services.AddScoped<IDashboardContributor, FinanceDashboardContributor>();

builder.Services.AddScoped<IModuleDescriptor, LifeAdminModule>();
builder.Services.AddScoped<ISearchable, LifeAdminSearchProvider>();
builder.Services.AddScoped<IDashboardContributor, LifeAdminDashboardContributor>();

// --- Localization (Shell provides the mechanism, each module owns its own .resx -
// see Docs/02_Pravila_Programiranja.md, section 5) ---
builder.Services.AddLocalization();

// --- Real-time (SignalR) - one household-scoped hub; a global filter turns
// successful mutations into "dataChanged" broadcasts so connected members see
// changes without a manual reload (Docs/00 - "Sinhronizacija u realnom vremenu").
builder.Services.AddSignalR();
builder.Services.AddScoped<HouseholdBroadcastFilter>();

// --- MVC + Razor Pages (Identity default UI uses Razor Pages) ---
builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<HouseholdBroadcastFilter>();
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();
builder.Services.AddRazorPages();

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    // New languages are added here, in one place (Docs/02_Pravila_Programiranja.md, section 5.4).
    var supportedCultures = new[] { "en", "bs" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);

    // English is always the fallback culture (section 5.4) - a missing key in
    // .bs.resx silently falls back to .en.resx instead of showing an error.
    options.FallBackToParentUICultures = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.MapHub<HouseholdHub>("/hubs/household");

app.Run();
