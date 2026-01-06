using GardenHub.Models;

namespace GardenHub.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<bool> UpgradeToProAsync(AppUser user, string paymentMethodId);
        Task<bool> CancelSubscriptionAsync(AppUser user);
        Task<bool> ReactivateSubscriptionAsync(AppUser user);
        Task<bool> ProcessPaymentAsync(AppUser user);
        Task CheckAndUpdateExpiredSubscriptionsAsync();
        Task<bool> IsSubscriptionActiveAsync(string userId);
    }
}