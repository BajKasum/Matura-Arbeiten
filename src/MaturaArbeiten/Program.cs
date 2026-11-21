using MaturaArbeiten.Data;
using MaturaArbeiten.Filters;
using MaturaArbeiten.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<PasswortAendernFilter>();
    ModellbindungMeldungen.Setzen(options.ModelBindingMessageProvider);
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPasswordHasher<string>, PasswordHasher<string>>();
builder.Services.AddScoped<IAnmeldeService, AnmeldeService>();
builder.Services.AddScoped<IPraesentationService, PraesentationService>();
builder.Services.AddScoped<IPraesentationVerwaltungService, PraesentationVerwaltungService>();
builder.Services.AddScoped<IEintragungService, EintragungService>();
builder.Services.AddScoped<IImportService, ImportService>();
builder.Services.AddScoped<IZugangsdatenService, ZugangsdatenService>();
builder.Services.AddScoped<IAdministrationService, AdministrationService>();
builder.Services.AddScoped<IG3SuSVerwaltungService, G3SuSVerwaltungService>();
builder.Services.AddScoped<IAdminLoginService, AdminLoginService>();
builder.Services.AddScoped<IBestaetigungService, BestaetigungService>();
builder.Services.AddScoped<IEmailProtokollService, EmailProtokollService>();
builder.Services.AddScoped<IListenService, ListenService>();
builder.Services.AddScoped<IPasswortService, PasswortService>();
builder.Services.AddSingleton<IEmailService, KonsolenEmailService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Anmelden";
        options.AccessDeniedPath = "/Account/KeinZugriff";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Events.OnValidatePrincipal = KontoPruefung.Pruefen;
    });

builder.Services
    .AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

var app = builder.Build();

using (var bereich = app.Services.CreateScope())
{
    try
    {
        Startdaten.ErstesAdministrationslogin(bereich.ServiceProvider);
    }
    catch (Exception fehler)
    {
        app.Logger.LogError(fehler, "Das erste Administrationslogin konnte nicht angelegt werden.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.Use(async (kontext, weiter) =>
{
    kontext.Response.Headers["X-Content-Type-Options"] = "nosniff";
    kontext.Response.Headers["Referrer-Policy"] = "no-referrer";
    kontext.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; img-src 'self' data:; frame-ancestors 'self'; form-action 'self'; base-uri 'self'";

    await weiter();
});

app.UseStatusCodePagesWithReExecute("/Home/Status", "?code={0}");
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();