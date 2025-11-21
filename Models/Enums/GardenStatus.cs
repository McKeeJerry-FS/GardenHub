using System.ComponentModel.DataAnnotations;

namespace GardenHub.Models.Enums
{
    public enum GardenStatus
    {
        [Display(Name = "Opened for the Day")]
        Opened = 1,
        [Display(Name = "Closed for the Day")]
        Closed = 2,
        [Display(Name = "Active - In Use")]
        Active = 3,
        [Display(Name = "Inactive")]
        Inactive = 4
    }
}