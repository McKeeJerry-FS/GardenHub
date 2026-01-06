using GardenHub.Services;
using GardenHub.Services.Interfaces;

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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking expired subscriptions");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}