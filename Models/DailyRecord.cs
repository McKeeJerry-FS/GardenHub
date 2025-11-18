using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardenHub.Models
{
    public class DailyRecord
    {
        [Key]
        public int RecordId { get; set; }

        [ForeignKey("Garden")]
        public int GardenId { get; set; }
        
        [Required]
        public Garden Garden { get; set; } = null!;

        [Required]
        [Display(Name = "Record Date")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Inside Temperature")]
        public double InsideTemperature { get; set; }
        [Display(Name = "Outside Temperature")]
        public double OutsideTemperature { get; set; }
        [Display(Name = "Inside Humidity")]
        public double InsideHumidity { get; set; }
        [Display(Name = "Outside Humidity")]
        public double OutsideHumidity { get; set; }
        
        [Display(Name = "Inside VPD")]
        public double InsideVPD { get; set; }
        [Display(Name = "Outside VPD")]
        public double OutsideVPD { get; set; }

        [Display(Name = "Lighting On")]
        public double LightingOn { get; set; }
        [Display(Name = "Lighting Off")]
        public double LightingOff { get; set; }
        
        [Display(Name = "Lighting Intensity")]
        public int LightingIntensity { get; set; }

        [Range(0, 100)]
        public int WaterAmount { get; set; }

        [Range(0, 100)]
        public int NutrientAmount { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;


        // Foreign key to AppUser

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public virtual AppUser? User { get; set; }
    }
}
