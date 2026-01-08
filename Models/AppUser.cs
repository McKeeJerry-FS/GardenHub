using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using GardenHub.Models.Enums;

namespace GardenHub.Models
{
    public class AppUser : IdentityUser
    {
        [Required]
        [StringLength(30, ErrorMessage = "First name must be between {2} and {0} characters long.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(30, ErrorMessage = "Last name must be between {2} and {0} characters long.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";


        // Properties for User Tiering
        public UserTier Tier { get; set; } = UserTier.Hobby;
        public DateTime? ProTierStartDate { get; set; }
        public DateTime? ProTierEndDate { get; set; }
        public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.None;
        
        // Cancellation tracking
        public DateTime? CancellationRequestedDate { get; set; }
        public DateTime? GracePeriodEndDate { get; set; }

        // Trial tracking
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }

        // Payment tracking
        public string? StripeCustomerId { get; set; }
        public string? StripeSubscriptionId { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? NextBillingDate { get; set; }

        // Navigation property for Payments
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        // Developer mode
        public bool IsDeveloperMode => Tier == UserTier.Developer;

        // Computed properties
        public bool IsProTierActive => Tier == UserTier.Pro && 
                                       SubscriptionStatus == SubscriptionStatus.Active &&
                                       ProTierEndDate.HasValue && 
                                       ProTierEndDate.Value > DateTime.UtcNow;

        public bool IsInGracePeriod => SubscriptionStatus == SubscriptionStatus.Cancelled &&
                                       GracePeriodEndDate.HasValue &&
                                       GracePeriodEndDate.Value > DateTime.UtcNow;

        public bool IsOnTrial => SubscriptionStatus == SubscriptionStatus.Trialing &&
                                 TrialEndDate.HasValue &&
                                 TrialEndDate.Value > DateTime.UtcNow;
                                

        public bool CanAccessProFeatures => IsDeveloperMode || IsProTierActive || IsInGracePeriod;
    }
}
