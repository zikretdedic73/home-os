using HomeOS.Data;
using HomeOS.Services;
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

// --- Localization (Shell provides the mechanism, each module owns its own .resx -
// see Docs/02_Pravila_Programiranja.md, section 5) ---
builder.Services.AddLocalization();

// --- MVC + Razor Pages (Identity default UI uses Razor Pages) ---
builder.Services.AddControllersWithViews()
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

app.Run();
