using System.ComponentModel.DataAnnotations;

namespace GardenHub.Models.Enums
{
    public enum PlantInspectionStatus
    {
        [Display(Name = "All Healthy")]
        AllHealthy = 1,
        [Display(Name = "Minor Issues Detected")]
        MinorIssues = 2,
        [Display(Name = "Action Required")]
        ActionRequired = 3,
        [Display(Name = "Critical - Immediate Attention")]
        Critical = 4
    }
}