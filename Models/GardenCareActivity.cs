using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GardenHub.Models.Enums;

namespace GardenHub.Models
{
    public class GardenCareActivity
    {
        [Key]
        public int ActivityId { get; set; }

        [ForeignKey("Garden")]
        public int GardenId { get; set; }
        
        public virtual Garden? Garden { get; set; }

        [Required]
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; }

        [Required]
        [Display(Name = "Activity Type")]
        public CareActivityType ActivityType { get; set; }

        [Display(Name = "Visual Inspection - Plants")]
        public PlantInspectionStatus? PlantInspectionStatus { get; set; }

        [Display(Name = "Visual Inspection - Equipment")]
        public EquipmentInspectionStatus? EquipmentInspectionStatus { get; set; }

        [Display(Name = "Water Level (Gallons)")]
        [Range(0, 10000)]
        public double? WaterLevel { get; set; }

        [Display(Name = "Watering Performed")]
        public bool WateringPerformed { get; set; }

        [Display(Name = "Water Amount (Gallons)")]
        [Range(0, 1000)]
        public double? WaterAmountAdded { get; set; }

        [Display(Name = "Nutrients Added")]
        public bool NutrientsAdded { get; set; }

        [Display(Name = "Nutrient Type")]
        [StringLength(200)]
        public string? NutrientType { get; set; }

        [Display(Name = "Nutrient Amount (ml/oz)")]
        [Range(0, 10000)]
        public double? NutrientAmount { get; set; }

        [Display(Name = "New Plantings")]
        public bool NewPlantingsAdded { get; set; }

        [Display(Name = "Number of Plants Added")]
        [Range(0, 1000)]
        public int? NumberOfPlantsAdded { get; set; }

        [Display(Name = "Plant Names/Types Added")]
        [StringLength(500)]
        public string? PlantTypesAdded { get; set; }

        [Display(Name = "Weeding Performed")]
        public bool WeedingPerformed { get; set; }

        [Display(Name = "Weeding Duration (minutes)")]
        [Range(0, 480)]
        public int? WeedingDuration { get; set; }

        [Display(Name = "Pruning/Trimming")]
        public bool PruningPerformed { get; set; }

        [Display(Name = "Pruning Notes")]
        [StringLength(500)]
        public string? PruningNotes { get; set; }

        [Display(Name = "Pest Control")]
        public bool PestControlPerformed { get; set; }

        [Display(Name = "Pest Control Details")]
        [StringLength(500)]
        public string? PestControlDetails { get; set; }

        [Display(Name = "pH Level")]
        [Range(0, 14)]
        public double? PhLevel { get; set; }

        [Display(Name = "EC/TDS Level (ppm)")]
        [Range(0, 5000)]
        public int? EcLevel { get; set; }

        [Display(Name = "Garden Status")]
        public GardenStatus? GardenStatus { get; set; }

        [Display(Name = "Duration (minutes)")]
        [Range(0, 960)]
        public int? ActivityDuration { get; set; }

        [Display(Name = "Activity Notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Image Properties
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public virtual AppUser? User { get; set; }
    }
}