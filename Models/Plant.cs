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
        public string PlantName { get; set; } = string.Empty;
        [Required]
        [StringLength(500, ErrorMessage = "Plant description must be between 2 and 500 characters long.", MinimumLength = 2)]
        public string PlantDescription { get; set; } = string.Empty;
        [Required]
        public PlantType PlantType { get; set; }
        [Required]
        public GrowMethod GrowMethod { get; set; }
        
        [Display(Name = "Date Planted")]
        public DateTime DatePlanted { get; set; }
        
        // Foreign Key to Garden
        [ForeignKey("Garden")]
        public int GardenId { get; set; }
        public virtual Garden Garden { get; set; } = null!;

        // Foreign key to AppUser
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser? User { get; set; }
    }
}
