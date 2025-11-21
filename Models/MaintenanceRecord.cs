using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GardenHub.Models.Enums;

namespace GardenHub.Models
{
    public class MaintenanceRecord
    {
        [Key]
        public int MaintenanceRecordId { get; set; }

        [Required]
        [ForeignKey("Equipment")]
        public int EquipmentId { get; set; }
        public Equipment? Equipment { get; set; }

        [Required]
        [Display(Name = "Status")]
        public MaintenanceStatus Status { get; set; }

        [Required]
        [Display(Name = "Request Date")]
        public DateTime RequestDate { get; set; }

        [Display(Name = "Maintenance Start Date")]
        public DateTime? MaintenanceStartDate { get; set; }

        [Display(Name = "Maintenance End Date")]
        public DateTime? MaintenanceEndDate { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Requested By")]
        [ForeignKey("RequestedBy")]
        public string RequestedByUserId { get; set; } = string.Empty;
        public virtual AppUser? RequestedBy { get; set; }

        [NotMapped]
        [Display(Name = "Downtime Duration")]
        public TimeSpan? DowntimeDuration
        {
            get
            {
                if (MaintenanceStartDate.HasValue && MaintenanceEndDate.HasValue)
                {
                    return MaintenanceEndDate.Value - MaintenanceStartDate.Value;
                }
                return null;
            }
        }
    }
}