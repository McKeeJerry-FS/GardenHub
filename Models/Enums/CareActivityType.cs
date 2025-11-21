using System.ComponentModel.DataAnnotations;

namespace GardenHub.Models.Enums
{
    public enum CareActivityType
    {
        [Display(Name = "Visual Inspection")]
        VisualInspection = 1,
        [Display(Name = "Watering")]
        Watering = 2,
        [Display(Name = "Adding Nutrients")]
        AddingNutrients = 3,
        [Display(Name = "New Plantings")]
        NewPlantings = 4,
        [Display(Name = "Weeding")]
        Weeding = 5,
        [Display(Name = "Pruning/Trimming")]
        Pruning = 6,
        [Display(Name = "Pest Control")]
        PestControl = 7,
        [Display(Name = "Water Level Check")]
        WaterLevelCheck = 8,
        [Display(Name = "pH/EC Adjustment")]
        PhEcAdjustment = 9,
        [Display(Name = "Garden Opening")]
        GardenOpening = 10,
        [Display(Name = "Garden Closing")]
        GardenClosing = 11,
        [Display(Name = "Harvesting")]
        Harvesting = 12,
        [Display(Name = "General Maintenance")]
        GeneralMaintenance = 13
    }
}