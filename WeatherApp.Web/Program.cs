using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Stripe;
using WeatherApp.Domain;
using WeatherApp.Domain.IdentityModels;
using WeatherApp.Repository.Data;
using WeatherApp.Repository.Implementation;
using WeatherApp.Repository.Interface;
using WeatherApp.Service.Implementation;
using WeatherApp.Service.Interface;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddIdentity<WeatherAppUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.AddTransient<IEmailSender, WeatherApp.Web.EmailSender>();


builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddTransient<ILocationService, LocationService>();
builder.Services.AddTransient<IUserSettingsService, UserSettingsService>();
builder.Services.AddTransient<IWeatherAlertService, WeatherAlertService>();
builder.Services.AddTransient<IWeatherDataService, WeatherDataService>();
builder.Services.AddTransient<IWeatherApiService, WeatherApiService>();
builder.Services.AddTransient<IOutfitRecommendationService, OutfitRecommendationService>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl"; 
});

Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
