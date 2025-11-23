using System.ComponentModel.DataAnnotations;

namespace GardenHub.Models.Enums
{
    public enum PlantCareActivityType
    {
        [Display(Name = "Initial Planting")]
        InitialPlanting = 1,
        [Display(Name = "Watering")]
        Watering = 2,
        [Display(Name = "Fertilizing")]
        Fertilizing = 3,
        [Display(Name = "Light Adjustment")]
        LightAdjustment = 4,
        [Display(Name = "Pruning/Trimming")]
        Pruning = 5,
        [Display(Name = "Support/Staking")]
        SupportStaking = 6,
        [Display(Name = "Pest Control")]
        PestControl = 7,
        [Display(Name = "Disease Treatment")]
        DiseaseTreatment = 8,
        [Display(Name = "Growth Monitoring")]
        GrowthMonitoring = 9,
        [Display(Name = "Environmental Check")]
        EnvironmentalCheck = 10,
        [Display(Name = "Transplanting")]
        Transplanting = 11,
        [Display(Name = "Harvesting")]
        Harvesting = 12,
        [Display(Name = "Plant Removal")]
        PlantRemoval = 13,
        [Display(Name = "General Maintenance")]
        GeneralMaintenance = 14
    }
}