using GardenHub.Models;

namespace GardenHub.Services.Interfaces
{
    public interface ISubscriptionEmailService
    {
        // Subscription Lifecycle
        Task SendTrialStartedEmailAsync(AppUser user);
        Task SendTrialEndingReminderEmailAsync(AppUser user, int daysRemaining);
        Task SendTrialExpiredEmailAsync(AppUser user);
        Task SendSubscriptionActivatedEmailAsync(AppUser user);
        Task SendSubscriptionCancelledEmailAsync(AppUser user);
        Task SendSubscriptionReactivatedEmailAsync(AppUser user);
        
        // Payment Events
        Task SendPaymentSuccessEmailAsync(AppUser user, decimal amount, string currency);
        Task SendPaymentFailedEmailAsync(AppUser user, string reason);
        Task SendPaymentRetryNotificationEmailAsync(AppUser user, DateTime nextRetryDate);
        
        // Grace Period & Warnings
        Task SendGracePeriodStartedEmailAsync(AppUser user, DateTime gracePeriodEnd);
        Task SendGracePeriodEndingEmailAsync(AppUser user, int daysRemaining);
        Task SendSubscriptionExpiredEmailAsync(AppUser user);
        
        // Payment Reminders
        Task SendUpcomingPaymentReminderEmailAsync(AppUser user, DateTime paymentDate, decimal amount);
        Task SendPaymentReceiptEmailAsync(AppUser user, string transactionId, decimal amount, string currency);
        
        // Refunds
        Task SendRefundProcessedEmailAsync(AppUser user, decimal amount, string reason);
    }
}