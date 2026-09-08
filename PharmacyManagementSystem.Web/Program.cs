using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString(
        "DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection is not configured.");
}


// ==========================================
// DATABASE
// ==========================================

PharmacyManagementSystem.Web.Database.DatabaseConnection
    .Initialize(connectionString);


// ==========================================
// SERVICES
// ==========================================

builder.Services.AddControllersWithViews();

builder.Services.AddSession();


// ==========================================
// COOKIE AUTHENTICATION
// ==========================================

builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";

        options.LogoutPath = "/Account/Logout";

        options.AccessDeniedPath =
            "/Account/AccessDenied";

        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        options.SlidingExpiration = true;
    });


// ==========================================
// AUTHORIZATION
// ==========================================

builder.Services.AddAuthorization();


var app = builder.Build();


// ==========================================
// HTTP REQUEST PIPELINE
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}


// ==========================================
// STATIC FILES
// ==========================================

app.UseStaticFiles();


// ==========================================
// ROUTING
// ==========================================

app.UseRouting();


// ==========================================
// SESSION
// ==========================================

app.UseSession();


// ==========================================
// AUTHENTICATION
// ==========================================

// Authentication MUST come before Authorization.

app.UseAuthentication();

app.UseAuthorization();


// ==========================================
// ROUTES
// ==========================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// ==========================================
// RUN
// ==========================================

app.Run();