using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

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
    }
}
