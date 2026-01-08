using GardenHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Security.Claims;

namespace GardenHub.Helpers
{
    [HtmlTargetElement("pro-feature")]
    public class ProFeatureTagHelper : TagHelper
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProFeatureTagHelper(UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        [HtmlAttributeName("feature-name")]
        public string? FeatureName { get; set; }

        [HtmlAttributeName("show-upgrade-prompt")]
        public bool ShowUpgradePrompt { get; set; } = true;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                output.SuppressOutput();
                return;
            }

            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                output.SuppressOutput();
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.CanAccessProFeatures)
            {
                if (ShowUpgradePrompt)
                {
                    output.TagName = "div";
                    output.Attributes.SetAttribute("class", "pro-feature-gate");
                    output.Content.SetHtmlContent($@"
                        <div class='alert alert-warning'>
                            <h5><i class='bi bi-crown-fill'></i> Pro Feature: {FeatureName ?? "Premium Feature"}</h5>
                            <p>Upgrade to Pro to access this feature.</p>
                            <a href='/Identity/Account/Manage/Subscription' class='btn btn-warning'>
                                <i class='bi bi-crown-fill'></i> Upgrade Now
                            </a>
                        </div>
                    ");
                }
                else
                {
                    output.SuppressOutput();
                }
                return;
            }

            output.TagName = null; // Remove the wrapper tag
        }
    }
}
