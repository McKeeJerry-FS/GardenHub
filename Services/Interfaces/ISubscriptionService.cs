using GardenHub.Models;

namespace GardenHub.Services.Interfaces
{
    public interface ISubscriptionService
    {
        // EXISTING METHODS
        Task<bool> StartTrialAsync(AppUser user);
        Task<bool> UpgradeToProAsync(AppUser user, string paymentMethodId);
        Task<bool> CancelSubscriptionAsync(AppUser user);
        Task<bool> ReactivateSubscriptionAsync(AppUser user);
        Task<bool> ProcessPaymentAsync(AppUser user);
        Task CheckAndUpdateExpiredSubscriptionsAsync();
        Task<bool> IsSubscriptionActiveAsync(string userId);

        // NEW METHODS NEEDED BY WEBHOOK CONTROLLER
        Task ActivateSubscriptionAsync(string userId, string subscriptionId, string customerId);
        Task HandleFailedPaymentAsync(string userId);
        Task CreateSubscriptionRecordAsync(string userId, string subscriptionId, string customerId, string status, DateTime periodEnd);
        Task UpdateSubscriptionStatusAsync(string subscriptionId, string status, DateTime periodEnd);
        Task MarkSubscriptionForCancellationAsync(string subscriptionId, DateTime cancelAt);
        Task DeactivateSubscriptionAsync(string subscriptionId);
        Task NotifyTrialEndingAsync(string userId, DateTime trialEnd);
        Task<string?> GetUserIdBySubscriptionIdAsync(string subscriptionId);
        Task ExtendSubscriptionAsync(string subscriptionId, DateTime periodEnd);
        Task StartGracePeriodAsync(string userId);
        Task CancelSubscriptionDueToFailedPaymentAsync(string subscriptionId);
        Task RemoveCustomerReferenceAsync(string userId);
    }
}