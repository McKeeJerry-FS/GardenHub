using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace GardenHub.Services.Authorization
{
    public class TierAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        public TierAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
        }

        public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            var policy = await base.GetPolicyAsync(policyName);

            if (policy != null)
            {
                return policy;
            }

            // Handle dynamic tier policies
            if (policyName.StartsWith("Tier:", StringComparison.OrdinalIgnoreCase))
            {
                var tierName = policyName.Substring("Tier:".Length);
                
                var policyBuilder = new AuthorizationPolicyBuilder();
                policyBuilder.RequireAuthenticatedUser();
                policyBuilder.AddRequirements(new TierRequirement(tierName));
                
                return policyBuilder.Build();
            }

            return null;
        }
    }

    public class TierRequirement : IAuthorizationRequirement
    {
        public string TierName { get; }

        public TierRequirement(string tierName)
        {
            TierName = tierName;
        }
    }
}