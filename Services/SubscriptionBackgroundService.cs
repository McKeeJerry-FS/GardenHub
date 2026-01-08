using GardenHub.Data;
using GardenHub.Models.Enums;
using GardenHub.Services;
using GardenHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class SubscriptionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SubscriptionBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

        public SubscriptionBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<SubscriptionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Subscription Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();
                    
                    await subscriptionService.CheckAndUpdateExpiredSubscriptionsAsync();
                    
                    _logger.LogInformation("Subscription check completed at {Time}", DateTime.UtcNow);

                    await SendEmailRemindersAsync(scope); // Send email reminders
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking expired subscriptions");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task SendEmailRemindersAsync(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<ISubscriptionEmailService>();
            
            // Send trial ending reminders (3 days before)
            var trialEndingSoon = await context.Users
                .Where(u => u.SubscriptionStatus == SubscriptionStatus.Trialing &&
                           u.TrialEndDate.HasValue &&
                           u.TrialEndDate.Value.Date == DateTime.UtcNow.Date.AddDays(3))
                .ToListAsync();

            foreach (var user in trialEndingSoon)
            {
                await emailService.SendTrialEndingReminderEmailAsync(user, 3);
            }

            // Send grace period ending reminders (2 days before)
            var gracePeriodEnding = await context.Users
                .Where(u => u.SubscriptionStatus == SubscriptionStatus.PastDue &&
                           u.GracePeriodEndDate.HasValue &&
                           u.GracePeriodEndDate.Value.Date == DateTime.UtcNow.Date.AddDays(2))
                .ToListAsync();

            foreach (var user in gracePeriodEnding)
            {
                await emailService.SendGracePeriodEndingEmailAsync(user, 2);
            }

            // Send upcoming payment reminders (3 days before)
            var upcomingPayments = await context.Users
                .Where(u => u.SubscriptionStatus == SubscriptionStatus.Active &&
                           u.NextBillingDate.HasValue &&
                           u.NextBillingDate.Value.Date == DateTime.UtcNow.Date.AddDays(3))
                .ToListAsync();

            foreach (var user in upcomingPayments)
            {
                await emailService.SendUpcomingPaymentReminderEmailAsync(user, user.NextBillingDate.Value, 9.99m);
            }
        }
    }
}