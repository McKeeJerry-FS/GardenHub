using System.ComponentModel.DataAnnotations;
using GardenHub.Models;
using GardenHub.Services;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GardenHub.Areas.Identity.Pages.Account.Manage
{
    public class SubscriptionModel(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        ISubscriptionService subscriptionService) : PageModel
    {
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly SignInManager<AppUser> _signInManager = signInManager;
        private readonly ISubscriptionService _subscriptionService = subscriptionService;

        [TempData]
        public string? StatusMessage { get; set; }

        [BindProperty]
        public InputModel? Input { get; set; }

        public AppUser? CurrentUser { get; set; }

        public class InputModel
        {
            [Display(Name = "Payment Method")]
            public string? PaymentMethodId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpgradeAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // TODO: Replace with actual Stripe payment method ID from client
            var success = await _subscriptionService.UpgradeToProAsync(CurrentUser, Input.PaymentMethodId);

            if (success)
            {
                StatusMessage = "Successfully upgraded to Pro tier!";
                await _signInManager.RefreshSignInAsync(CurrentUser);
            }
            else
            {
                StatusMessage = "Error: Failed to upgrade to Pro tier.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var success = await _subscriptionService.CancelSubscriptionAsync(CurrentUser);

            if (success)
            {
                StatusMessage = $"Subscription cancelled. You'll have access to Pro features until {CurrentUser.GracePeriodEndDate:MMM dd, yyyy}.";
                await _signInManager.RefreshSignInAsync(CurrentUser);
            }
            else
            {
                StatusMessage = "Error: Failed to cancel subscription.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReactivateAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var success = await _subscriptionService.ReactivateSubscriptionAsync(CurrentUser);

            if (success)
            {
                StatusMessage = "Subscription reactivated successfully!";
                await _signInManager.RefreshSignInAsync(CurrentUser);
            }
            else
            {
                StatusMessage = "Error: Failed to reactivate subscription.";
            }

            return RedirectToPage();
        }
    }
}