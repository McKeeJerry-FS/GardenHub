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
        // TODO: Add Stripe service when payment integration is ready
        // private readonly IStripeService _stripeService;

        public SubscriptionService(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            ILogger<SubscriptionService> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<bool> UpgradeToProAsync(AppUser user, string paymentMethodId)
        {
            try
            {
                // TODO: Process payment with Stripe
                // var subscription = await _stripeService.CreateSubscriptionAsync(user.Email, paymentMethodId);

                user.Tier = UserTier.Pro;
                user.SubscriptionStatus = SubscriptionStatus.Active;
                user.ProTierStartDate = DateTime.UtcNow;
                user.ProTierEndDate = DateTime.UtcNow.AddMonths(1); // Monthly billing
                user.LastPaymentDate = DateTime.UtcNow;
                user.NextBillingDate = DateTime.UtcNow.AddMonths(1);
                user.CancellationRequestedDate = null;
                user.GracePeriodEndDate = null;

                // TODO: Store Stripe customer and subscription IDs
                // user.StripeCustomerId = subscription.CustomerId;
                // user.StripeSubscriptionId = subscription.Id;

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
                if (user.Tier != UserTier.Pro || user.SubscriptionStatus != SubscriptionStatus.Active)
                {
                    return false;
                }

                // TODO: Cancel Stripe subscription at period end
                // await _stripeService.CancelSubscriptionAsync(user.StripeSubscriptionId);

                user.SubscriptionStatus = SubscriptionStatus.Cancelled;
                user.CancellationRequestedDate = DateTime.UtcNow;
                user.GracePeriodEndDate = DateTime.UtcNow.AddDays(14); // 14-day courtesy period

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} cancelled Pro subscription. Grace period ends {GraceEnd}", 
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

                // TODO: Reactivate Stripe subscription
                // await _stripeService.ReactivateSubscriptionAsync(user.StripeSubscriptionId);

                user.SubscriptionStatus = SubscriptionStatus.Active;
                user.CancellationRequestedDate = null;
                user.GracePeriodEndDate = null;
                user.ProTierEndDate = user.NextBillingDate;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserId} reactivated Pro subscription", user.Id);
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

                // TODO: Process payment with Stripe
                // var paymentResult = await _stripeService.ProcessPaymentAsync(user.StripeSubscriptionId);

                // if (paymentResult.Success)
                // {
                user.LastPaymentDate = DateTime.UtcNow;
                user.NextBillingDate = DateTime.UtcNow.AddMonths(1);
                user.ProTierEndDate = DateTime.UtcNow.AddMonths(1);

                var result = await _userManager.UpdateAsync(user);
                return result.Succeeded;
                // }
                // else
                // {
                //     user.SubscriptionStatus = SubscriptionStatus.PastDue;
                //     await _userManager.UpdateAsync(user);
                //     return false;
                // }
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
            var expiredUsers = await _context.Users
                .Where(u => u.SubscriptionStatus == SubscriptionStatus.Cancelled &&
                           u.GracePeriodEndDate.HasValue &&
                           u.GracePeriodEndDate.Value <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var user in expiredUsers)
            {
                user.Tier = UserTier.Hobby;
                user.SubscriptionStatus = SubscriptionStatus.Expired;
                user.ProTierEndDate = DateTime.UtcNow;
                
                _logger.LogInformation("User {UserId} grace period expired, reverted to Hobby tier", user.Id);
            }

            if (expiredUsers.Any())
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
