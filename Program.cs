using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Services;
using GardenHub.Services.Authorization;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration);
if (string.IsNullOrEmpty(connectionString))
{
    // Log environment variables for debugging
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var pgHost = Environment.GetEnvironmentVariable("PGHOST");
    var pgDatabase = Environment.GetEnvironmentVariable("PGDATABASE");

    throw new InvalidOperationException($"Database connection string is required. DATABASE_URL: {databaseUrl}, PGHOST: {pgHost}, PGDATABASE: {pgDatabase}");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)
           .ConfigureWarnings(warnings => 
               warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // Pro Tier Policy
    options.AddPolicy("RequireProTier", policy =>
        policy.Requirements.Add(new ProTierRequirement()));

    // Hobby Tier Policy
    options.AddPolicy("Tier:Hobby", policy =>
        policy.Requirements.Add(new TierRequirement("Hobby")));

    // Pro Tier Policy (alternative syntax)
    options.AddPolicy("Tier:Pro", policy =>
        policy.Requirements.Add(new TierRequirement("Pro")));
});

// Register Authorization Handlers
builder.Services.AddScoped<IAuthorizationHandler, ProTierAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TierAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationPolicyProvider, TierAuthorizationPolicyProvider>();

builder.Services.AddControllersWithViews();

//  DI Container registrations
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IGardenService, GardenService>();
builder.Services.AddScoped<IDailyRecordService, DailyRecordsService>();
builder.Services.AddScoped<IJournalEntriesService, JournalEntriesService>();
builder.Services.AddScoped<IPlantService, PlantService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IGardenCareService, GardenCareService>();
builder.Services.AddScoped<IPlantCareService, PlantCareService>();
builder.Services.AddScoped<IReminderService, ReminderService>();

builder.Services.AddTransient<IEmailSender, EmailService>();

// Payment and Subscription Services
builder.Services.AddScoped<IStripeService, StripeService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddHostedService<SubscriptionBackgroundService>();
builder.Services.AddScoped<ISubscriptionEmailService, SubscriptionEmailService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UserManager<AppUser>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

// Apply database migrations and seed data (all environments)
using (var scope = app.Services.CreateScope())
{
    await DataUtility.ManageDataAsync(scope.ServiceProvider);
}

app.Run();
