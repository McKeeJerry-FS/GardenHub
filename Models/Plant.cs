using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GardenHub.Models.Enums;

namespace GardenHub.Models
{
    public class Plant
    {
        [Key]
        public int PlantId { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Plant name must be between 2 and 100 characters long.", MinimumLength = 2)]
        [Display(Name = "Plant Name")]
        public string PlantName { get; set; } = string.Empty;
        [Required]
        [StringLength(500, ErrorMessage = "Plant description must be between 2 and 500 characters long.", MinimumLength = 2)]
        [Display(Name = "Description")]
        public string PlantDescription { get; set; } = string.Empty;
        [Required]
        [Display(Name = "Plant Type")]
        public PlantType PlantType { get; set; }

        [Required]
        [Display(Name = "Lighting Requirement")]
        public LightingRequirement LightingRequirement { get; set; }

        [Required]
        [Display(Name = "Grow Method")]
        public GrowMethod GrowMethod { get; set; }
        
        [Display(Name = "Date Planted")]
        public DateTime DatePlanted { get; set; }

        [Display(Name = "Plant Condition")]
        public PlantCondition? PlantCondition { get; set; }

        // Image Properties
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        // Foreign Key to Garden
        [ForeignKey("Garden")]
        public int GardenId { get; set; }
        public virtual Garden? Garden { get; set; }

        // Foreign key to AppUser
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser? User { get; set; }

        // navigation property for plant care activities
        public virtual ICollection<PlantCareActivity> CareActivities { get; set; } = new List<PlantCareActivity>();
    }
}
