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

        public SubscriptionService(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            ILogger<SubscriptionService> logger,
            IStripeService stripeService,
            IPaymentService paymentService,
            IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _stripeService = stripeService;
            _paymentService = paymentService;
            _configuration = configuration;
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

                // Create payment record
                await _paymentService.CreatePaymentRecordAsync(
                    user.Id,
                    subscription.LatestInvoice?.Id ?? $"sub_{subscription.Id}",
                    9.99m, // Pro tier price
                    UserTier.Pro,
                    "Pro Tier Subscription - Monthly"
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
    }
}
