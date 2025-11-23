using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GardenHub.Models.Enums;

namespace GardenHub.Models
{
    public class PlantCareActivity
    {
        [Key]
        public int ActivityId { get; set; }

        [ForeignKey("Plant")]
        public int PlantId { get; set; }
        
        public virtual Plant? Plant { get; set; }

        [Required]
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; }

        [Required]
        [Display(Name = "Activity Type")]
        public PlantCareActivityType ActivityType { get; set; }

        // Initial Planting
        [Display(Name = "Planting Depth (inches)")]
        [Range(0, 100)]
        public double? PlantingDepth { get; set; }

        [Display(Name = "Seed/Transplant")]
        [StringLength(100)]
        public string? PlantingMethod { get; set; }

        [Display(Name = "Growing Medium")]
        [StringLength(200)]
        public string? GrowingMedium { get; set; }

        // Watering
        [Display(Name = "Watering Performed")]
        public bool WateringPerformed { get; set; }

        [Display(Name = "Water Amount (ml/oz)")]
        [Range(0, 10000)]
        public double? WaterAmount { get; set; }

        [Display(Name = "Soil Moisture Level")]
        public MoistureLevel? MoistureLevel { get; set; }

        // Fertilizer/Nutrients
        [Display(Name = "Fertilizer Applied")]
        public bool FertilizerApplied { get; set; }

        [Display(Name = "Fertilizer Type")]
        [StringLength(200)]
        public string? FertilizerType { get; set; }

        [Display(Name = "Fertilizer Amount (ml/g)")]
        [Range(0, 1000)]
        public double? FertilizerAmount { get; set; }

        [Display(Name = "NPK Ratio")]
        [StringLength(50)]
        public string? NpkRatio { get; set; }

        // Light Access
        [Display(Name = "Light Hours Per Day")]
        [Range(0, 24)]
        public double? LightHours { get; set; }

        [Display(Name = "Light Source")]
        public LightSource? LightSource { get; set; }

        [Display(Name = "Light Intensity")]
        public LightIntensity? LightIntensity { get; set; }

        // Maintenance
        [Display(Name = "Pruning Performed")]
        public bool PruningPerformed { get; set; }

        [Display(Name = "Pruning Details")]
        [StringLength(500)]
        public string? PruningDetails { get; set; }

        [Display(Name = "Support/Staking Added")]
        public bool SupportAdded { get; set; }

        [Display(Name = "Support Details")]
        [StringLength(300)]
        public string? SupportDetails { get; set; }

        [Display(Name = "Pest Control Performed")]
        public bool PestControlPerformed { get; set; }

        [Display(Name = "Pest/Disease Identified")]
        [StringLength(300)]
        public string? PestDiseaseIdentified { get; set; }

        [Display(Name = "Treatment Applied")]
        [StringLength(500)]
        public string? TreatmentApplied { get; set; }

        // Growth Conditions
        [Display(Name = "Temperature (°F)")]
        [Range(-50, 150)]
        public double? Temperature { get; set; }

        [Display(Name = "Humidity (%)")]
        [Range(0, 100)]
        public int? Humidity { get; set; }

        [Display(Name = "pH Level")]
        [Range(0, 14)]
        public double? PhLevel { get; set; }

        [Display(Name = "Plant Health Status")]
        public PlantHealthStatus? PlantHealthStatus { get; set; }

        // Growth Tracking
        [Display(Name = "Height (inches)")]
        [Range(0, 1000)]
        public double? PlantHeight { get; set; }

        [Display(Name = "Leaf Count")]
        [Range(0, 10000)]
        public int? LeafCount { get; set; }

        [Display(Name = "Flowering/Fruiting")]
        public bool IsFloweringFruiting { get; set; }

        [Display(Name = "Flowering/Fruiting Details")]
        [StringLength(300)]
        public string? FloweringFruitingDetails { get; set; }

        // Harvest
        [Display(Name = "Harvest Performed")]
        public bool HarvestPerformed { get; set; }

        [Display(Name = "Harvest Amount")]
        [StringLength(100)]
        public string? HarvestAmount { get; set; }

        [Display(Name = "Harvest Quality")]
        public HarvestQuality? HarvestQuality { get; set; }

        // End of Life
        [Display(Name = "Plant Removed")]
        public bool PlantRemoved { get; set; }

        [Display(Name = "Removal Reason")]
        [StringLength(500)]
        public string? RemovalReason { get; set; }

        // General
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