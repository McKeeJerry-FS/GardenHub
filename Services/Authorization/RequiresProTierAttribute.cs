using Microsoft.AspNetCore.Authorization;

namespace GardenHub.Services.Authorization
{
    /// <summary>
    /// Specifies that the class or method that this attribute is applied to requires Pro tier access.
    /// </summary>
    public class RequiresProTierAttribute : AuthorizeAttribute
    {
        public RequiresProTierAttribute()
        {
            Policy = "RequireProTier";
        }
    }

    /// <summary>
    /// Specifies that the class or method requires a specific tier.
    /// </summary>
    public class RequiresTierAttribute : AuthorizeAttribute
    {
        public RequiresTierAttribute(string tierName)
        {
            Policy = $"Tier:{tierName}";
        }
    }
}