using GardenHub.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace GardenHub.Services.Authorization
{
    public class ProTierRequirement : IAuthorizationRequirement
    {
    }

    public class ProTierAuthorizationHandler : AuthorizationHandler<ProTierRequirement>
    {
        private readonly UserManager<AppUser> _userManager;

        public ProTierAuthorizationHandler(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProTierRequirement requirement)
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

            // Developer mode has access to all Pro features
            if (user.IsDeveloperMode)
            {
                context.Succeed(requirement);
                return;
            }

            // Check if user has access to Pro features
            if (user.CanAccessProFeatures)
            {
                context.Succeed(requirement);
            }
        }
    }
}