using System.ComponentModel.DataAnnotations;

namespace GardenHub.Models.Enums
{
    public enum MoistureLevel
    {
        [Display(Name = "Dry")]
        Dry = 1,
        [Display(Name = "Slightly Moist")]
        SlightlyMoist = 2,
        [Display(Name = "Moist")]
        Moist = 3,
        [Display(Name = "Very Moist")]
        VeryMoist = 4,
        [Display(Name = "Saturated")]
        Saturated = 5
    }

    public enum LightSource
    {
        [Display(Name = "Natural Sunlight")]
        NaturalSunlight = 1,
        [Display(Name = "LED Grow Light")]
        LedGrowLight = 2,
        [Display(Name = "Fluorescent Light")]
        FluorescentLight = 3,
        [Display(Name = "HID Light")]
        HidLight = 4,
        [Display(Name = "Mixed Lighting")]
        MixedLighting = 5
    }

    public enum LightIntensity
    {
        [Display(Name = "Low")]
        Low = 1,
        [Display(Name = "Medium")]
        Medium = 2,
        [Display(Name = "High")]
        High = 3,
        [Display(Name = "Very High")]
        VeryHigh = 4
    }

    public enum PlantHealthStatus
    {
        [Display(Name = "Excellent")]
        Excellent = 1,
        [Display(Name = "Good")]
        Good = 2,
        [Display(Name = "Fair")]
        Fair = 3,
        [Display(Name = "Poor")]
        Poor = 4,
        [Display(Name = "Critical")]
        Critical = 5
    }

    public enum HarvestQuality
    {
        [Display(Name = "Excellent")]
        Excellent = 1,
        [Display(Name = "Good")]
        Good = 2,
        [Display(Name = "Fair")]
        Fair = 3,
        [Display(Name = "Poor")]
        Poor = 4
    }
}