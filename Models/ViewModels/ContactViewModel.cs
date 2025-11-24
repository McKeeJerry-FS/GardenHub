using System.ComponentModel.DataAnnotations;

namespace GardenHub.Models.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Your Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Subject must be between 5 and 200 characters")]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public ContactCategory Category { get; set; }

        [Required(ErrorMessage = "Message is required")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Message must be between 10 and 2000 characters")]
        [Display(Name = "Message")]
        public string Message { get; set; } = string.Empty;
    }

    public enum ContactCategory
    {
        [Display(Name = "General Inquiry")]
        GeneralInquiry,
        
        [Display(Name = "Bug Report")]
        BugReport,
        
        [Display(Name = "Feature Request")]
        FeatureRequest,
        
        [Display(Name = "Technical Support")]
        TechnicalSupport,
        
        [Display(Name = "Feedback")]
        Feedback,
        
        [Display(Name = "Other")]
        Other
    }
}