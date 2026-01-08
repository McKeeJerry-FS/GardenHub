using GardenHub.Models;
using GardenHub.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GardenHub.Services.Authorization
{
    public class TierAuthorizationHandler : AuthorizationHandler<TierRequirement>
    {
        private readonly UserManager<AppUser> _userManager;

        public TierAuthorizationHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TierRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return;
            }

            // Developer mode bypasses all tier restrictions
            if (user.IsDeveloperMode)
            {
                context.Succeed(requirement);
                return;
            }

            // Check tier access
            var requiredTier = Enum.Parse<UserTier>(requirement.TierName, ignoreCase: true);

            if (requiredTier == UserTier.Hobby)
            {
                // Everyone has access to Hobby tier features
                context.Succeed(requirement);
            }
            else if (requiredTier == UserTier.Pro)
            {
                // Only users with Pro access
                if (user.CanAccessProFeatures)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}