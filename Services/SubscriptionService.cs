using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<SubscriptionService> _logger;
        private readonly IStripeService _stripeService;
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;
        private readonly ISubscriptionEmailService _emailService;

        public SubscriptionService(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            ILogger<SubscriptionService> logger,
            IStripeService stripeService,
            IPaymentService paymentService,
            IConfiguration configuration,
            ISubscriptionEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _stripeService = stripeService;
            _paymentService = paymentService;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<bool> StartTrialAsync(AppUser user)
        {
            try
            {
                user.Tier = UserTier.Pro;
                user.SubscriptionStatus = SubscriptionStatus.Trialing;
                user.TrialStartDate = DateTime.UtcNow;
                user.TrialEndDate = DateTime.UtcNow.AddDays(14); // 14-day trial
                user.ProTierStartDate = DateTime.UtcNow;
                user.ProTierEndDate = user.TrialEndDate;

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} started trial period", user.Id);
                    await _emailService.SendTrialStartedEmailAsync(user);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting trial for user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> UpgradeToProAsync(AppUser user, string paymentMethodId)
        {
            try
            {
                // Create Stripe customer if doesn't exist
                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    var customer = await _stripeService.CreateCustomerAsync(user);
                    user.StripeCustomerId = customer.Id;
                }

                // Update payment method
                await _stripeService.UpdatePaymentMethodAsync(user.StripeCustomerId, paymentMethodId);

                // Get Pro tier price ID from configuration
                var priceId = _configuration["Stripe:ProPriceId"];
                if (string.IsNullOrEmpty(priceId))
                {
                    throw new InvalidOperationException("Stripe Pro Price ID not configured");
                }

                // Create subscription
                var subscription = await _stripeService.CreateSubscriptionAsync(user.StripeCustomerId, priceId);

                // Update user subscription info
                user.Tier = UserTier.Pro;
                user.SubscriptionStatus = SubscriptionStatus.Active;
                user.StripeSubscriptionId = subscription.Id;
                user.ProTierStartDate = DateTime.UtcNow;
                user.ProTierEndDate = DateTime.UtcNow.AddMonths(1);
                user.LastPaymentDate = DateTime.UtcNow;
                user.NextBillingDate = DateTime.UtcNow.AddMonths(1);
                user.CancellationRequestedDate = null;
                user.GracePeriodEndDate = null;

                // Clear trial dates if converting from trial
                if (user.IsOnTrial)
                {
                    user.TrialStartDate = null;
                    user.TrialEndDate = null;
                }

                // Record the successful payment
                await _paymentService.RecordSuccessfulPaymentAsync(
                    user.Id,
                    subscription.LatestInvoice?.Id ?? $"sub_{subscription.Id}",
                    9.99m, // Pro tier price
                    "usd",
                    subscription.LatestInvoice?.Id,
                    user.StripeCustomerId
                );

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} upgraded to Pro tier", user.Id);
                    return true;
                }

                _logger.LogError("Failed to upgrade user {UserId} to Pro tier", user.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error upgrading user {UserId} to Pro tier", user.Id);
                return false;
            }
        }

        public async Task<bool> CancelSubscriptionAsync(AppUser user)
        {
            try
            {
                if (user.Tier != UserTier.Pro || 
                    (user.SubscriptionStatus != SubscriptionStatus.Active && 
                     user.SubscriptionStatus != SubscriptionStatus.Trialing))
                {
                    return false;
                }

                // Cancel Stripe subscription at period end if not on trial
                if (!user.IsOnTrial && !string.IsNullOrEmpty(user.StripeSubscriptionId))
                {
                    await _stripeService.CancelSubscriptionAsync(user.StripeSubscriptionId, cancelImmediately: false);
                }

                user.SubscriptionStatus = SubscriptionStatus.Cancelled;
                user.CancellationRequestedDate = DateTime.UtcNow;
                user.GracePeriodEndDate = DateTime.UtcNow.AddDays(14); // 14-day courtesy period

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} cancelled subscription. Grace period ends {GraceEnd}", 
                        user.Id, user.GracePeriodEndDate);
                    await _emailService.SendSubscriptionCancelledEmailAsync(user);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription for user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> ReactivateSubscriptionAsync(AppUser user)
        {
            try
            {
                if (user.SubscriptionStatus != SubscriptionStatus.Cancelled || !user.IsInGracePeriod)
                {
                    return false;
                }

                // Reactivate Stripe subscription if it exists
                if (!string.IsNullOrEmpty(user.StripeSubscriptionId))
                {
                    await _stripeService.ReactivateSubscriptionAsync(user.StripeSubscriptionId);
                }

                user.SubscriptionStatus = SubscriptionStatus.Active;
                user.CancellationRequestedDate = null;
                user.GracePeriodEndDate = null;
                user.ProTierEndDate = user.NextBillingDate;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} reactivated subscription", user.Id);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating subscription for user {UserId}", user.Id);
                return false;
            }
        }

        public async Task<bool> ProcessPaymentAsync(AppUser user)
        {
            try
            {
                if (user.SubscriptionStatus != SubscriptionStatus.Active)
                {
                    return false;
                }

                // In production, this would be triggered by Stripe webhooks
                // This is a placeholder for manual testing
                user.LastPaymentDate = DateTime.UtcNow;
                user.NextBillingDate = DateTime.UtcNow.AddMonths(1);
                user.ProTierEndDate = DateTime.UtcNow.AddMonths(1);

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for user {UserId}", user.Id);
                user.SubscriptionStatus = SubscriptionStatus.PastDue;
                await _userManager.UpdateAsync(user);
                return false;
            }
        }

        public async Task CheckAndUpdateExpiredSubscriptionsAsync()
        {
            // Check expired trials
            var expiredTrials = await _context.Users
                .Where(u => u.SubscriptionStatus == SubscriptionStatus.Trialing &&
                           u.TrialEndDate.HasValue &&
                           u.TrialEndDate.Value <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var user in expiredTrials)
            {
                user.Tier = UserTier.Hobby;
                user.SubscriptionStatus = SubscriptionStatus.Expired;
                user.ProTierEndDate = DateTime.UtcNow;
                
                _logger.LogInformation("User {UserId} trial period expired, reverted to Hobby tier", user.Id);
            }

            // Check expired grace periods
            var expiredGracePeriods = await _context.Users
                .Where(u => u.SubscriptionStatus == SubscriptionStatus.Cancelled &&
                           u.GracePeriodEndDate.HasValue &&
                           u.GracePeriodEndDate.Value <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var user in expiredGracePeriods)
            {
                user.Tier = UserTier.Hobby;
                user.SubscriptionStatus = SubscriptionStatus.Expired;
                user.ProTierEndDate = DateTime.UtcNow;
                
                _logger.LogInformation("User {UserId} grace period expired, reverted to Hobby tier", user.Id);
            }

            if (expiredTrials.Any() || expiredGracePeriods.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsSubscriptionActiveAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user?.CanAccessProFeatures ?? false;
        }

        // NEW WEBHOOK-RELATED METHODS
        
        public async Task ActivateSubscriptionAsync(string userId, string subscriptionId, string customerId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"User {userId} not found");
            }

            user.StripeSubscriptionId = subscriptionId;
            user.StripeCustomerId = customerId;
            user.Tier = UserTier.Pro;
            user.SubscriptionStatus = SubscriptionStatus.Active;
            user.ProTierStartDate = DateTime.UtcNow;
            user.ProTierEndDate = DateTime.UtcNow.AddMonths(1);
            user.LastPaymentDate = DateTime.UtcNow;
            user.NextBillingDate = DateTime.UtcNow.AddMonths(1);

            // Clear trial if converting
            if (user.IsOnTrial)
            {
                user.TrialStartDate = null;
                user.TrialEndDate = null;
            }

            await _userManager.UpdateAsync(user);
            await _emailService.SendSubscriptionActivatedEmailAsync(user);
            _logger.LogInformation("Activated subscription {SubscriptionId} for user {UserId}", subscriptionId, userId);
        }

        public async Task HandleFailedPaymentAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            user.SubscriptionStatus = SubscriptionStatus.PastDue;
            await _userManager.UpdateAsync(user);
            
            _logger.LogWarning("Payment failed for user {UserId}, status set to PastDue", userId);
        }

        public async Task CreateSubscriptionRecordAsync(
            string userId, 
            string subscriptionId, 
            string customerId, 
            string status, 
            DateTime periodEnd)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            user.StripeSubscriptionId = subscriptionId;
            user.StripeCustomerId = customerId;
            user.SubscriptionStatus = MapStripeStatus(status);
            user.NextBillingDate = periodEnd;
            user.ProTierEndDate = periodEnd;

            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Created subscription record for user {UserId}", userId);
        }

        public async Task UpdateSubscriptionStatusAsync(string subscriptionId, string status, DateTime periodEnd)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.StripeSubscriptionId == subscriptionId);

            if (user == null)
            {
                _logger.LogWarning("No user found with subscription ID {SubscriptionId}", subscriptionId);
                return;
            }

            user.SubscriptionStatus = MapStripeStatus(status);
            user.NextBillingDate = periodEnd;
            user.ProTierEndDate = periodEnd;

            // Activate Pro tier if subscription is active
            if (status == "active")
            {
                user.Tier = UserTier.Pro;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated subscription {SubscriptionId} status to {Status}", subscriptionId, status);
        }

        public async Task MarkSubscriptionForCancellationAsync(string subscriptionId, DateTime cancelAt)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.StripeSubscriptionId == subscriptionId);

            if (user == null) return;

            user.CancellationRequestedDate = DateTime.UtcNow;
            user.ProTierEndDate = cancelAt;
            user.SubscriptionStatus = SubscriptionStatus.Cancelled;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Marked subscription {SubscriptionId} for cancellation at {CancelAt}", 
                subscriptionId, cancelAt);
        }

        public async Task DeactivateSubscriptionAsync(string subscriptionId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.StripeSubscriptionId == subscriptionId);

            if (user == null) return;

            user.Tier = UserTier.Hobby;
            user.SubscriptionStatus = SubscriptionStatus.Expired;
            user.ProTierEndDate = DateTime.UtcNow;
            user.StripeSubscriptionId = null;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Deactivated subscription {SubscriptionId}, user downgraded to Hobby", subscriptionId);
        }

        public async Task NotifyTrialEndingAsync(string userId, DateTime trialEnd)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            var daysRemaining = (trialEnd - DateTime.UtcNow).Days;
            await _emailService.SendTrialEndingReminderEmailAsync(user, daysRemaining);
            
            _logger.LogInformation("User {UserId} trial ending on {TrialEnd}", userId, trialEnd);
        }

        public async Task<string?> GetUserIdBySubscriptionIdAsync(string subscriptionId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.StripeSubscriptionId == subscriptionId);

            return user?.Id;
        }

        public async Task ExtendSubscriptionAsync(string subscriptionId, DateTime periodEnd)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.StripeSubscriptionId == subscriptionId);

            if (user == null) return;

            user.NextBillingDate = periodEnd;
            user.ProTierEndDate = periodEnd;
            user.LastPaymentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Extended subscription {SubscriptionId} to {PeriodEnd}", subscriptionId, periodEnd);
        }

        public async Task StartGracePeriodAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            user.SubscriptionStatus = SubscriptionStatus.PastDue;
            user.GracePeriodEndDate = DateTime.UtcNow.AddDays(7); // 7-day grace period

            await _userManager.UpdateAsync(user);
            await _emailService.SendGracePeriodStartedEmailAsync(user, user.GracePeriodEndDate.Value);
            _logger.LogInformation("Started grace period for user {UserId}, ends {GraceEnd}", 
                userId, user.GracePeriodEndDate);
        }

        public async Task CancelSubscriptionDueToFailedPaymentAsync(string subscriptionId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.StripeSubscriptionId == subscriptionId);

            if (user == null) return;

            user.Tier = UserTier.Hobby;
            user.SubscriptionStatus = SubscriptionStatus.Cancelled;
            user.ProTierEndDate = DateTime.UtcNow;
            user.StripeSubscriptionId = null;

            await _context.SaveChangesAsync();
            _logger.LogWarning("Cancelled subscription {SubscriptionId} due to failed payments", subscriptionId);
        }

        public async Task RemoveCustomerReferenceAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            user.StripeCustomerId = null;
            user.StripeSubscriptionId = null;

            await _userManager.UpdateAsync(user);
            _logger.LogInformation("Removed Stripe customer reference for user {UserId}", userId);
        }

        // HELPER METHODS

        private SubscriptionStatus MapStripeStatus(string stripeStatus)
        {
            return stripeStatus.ToLower() switch
            {
                "active" => SubscriptionStatus.Active,
                "trialing" => SubscriptionStatus.Trialing,
                "past_due" => SubscriptionStatus.PastDue,
                "canceled" => SubscriptionStatus.Cancelled,
                "unpaid" => SubscriptionStatus.PastDue,
                _ => SubscriptionStatus.Expired
            };
        }
    }
}
