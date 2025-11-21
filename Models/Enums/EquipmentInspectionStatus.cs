using System.ComponentModel.DataAnnotations;

namespace GardenHub.Models.Enums
{
    public enum EquipmentInspectionStatus
    {
        [Display(Name = "All Operational")]
        AllOperational = 1,
        [Display(Name = "Minor Issues")]
        MinorIssues = 2,
        [Display(Name = "Needs Maintenance")]
        NeedsMaintenance = 3,
        [Display(Name = "Equipment Failure")]
        Failure = 4
    }
}