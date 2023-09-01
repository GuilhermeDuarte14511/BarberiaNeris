using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Entities.Model;
using Business;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Google Authentication
builder.Services.AddSingleton(builder.Configuration);

// Configure Entity Framework Core
builder.Services.AddDbContext<BarbeariaContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21)))
    .EnableDetailedErrors());

builder.Services.AddScoped<AgendamentoBLL>();
builder.Services.AddTransient<HistoricoAgendamentosBLL>();

// Add Cookie Authentication services
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.Cookie.IsEssential = true; // This ensures the cookie is created regardless of privacy settings.
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Set the duration of the persistent cookie here. For example, 7 days.
    options.LoginPath = "/Login"; // Change to your login path if different.
    options.LogoutPath = "/Logout"; // Change to your logout path if different.
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // This must come before UseAuthorization()
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
