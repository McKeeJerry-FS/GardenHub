using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GardenHub.Models.Enums;

namespace GardenHub.Models
{
    public class Reminder
    {
        [Key]
        public int ReminderId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Reminder Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Reminder Date & Time")]
        public DateTime ReminderDateTime { get; set; }

        [Required]
        [Display(Name = "Reminder Type")]
        public ReminderType ReminderType { get; set; }

        [Display(Name = "Is Completed")]
        public bool IsCompleted { get; set; } = false;

        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Is Recurring")]
        public bool IsRecurring { get; set; } = false;

        [Display(Name = "Recurrence Pattern")]
        public RecurrencePattern? RecurrencePattern { get; set; }

        [Display(Name = "Recurrence Interval")]
        public int? RecurrenceInterval { get; set; }

        [Display(Name = "Priority")]
        public ReminderPriority Priority { get; set; } = ReminderPriority.Normal;

        // Relationships - Optional links to specific entities
        [Display(Name = "Daily Record")]
        [ForeignKey("DailyRecord")]
        public int? DailyRecordId { get; set; }
        public virtual DailyRecord? DailyRecord { get; set; }

        [Display(Name = "Garden Care Activity")]
        [ForeignKey("GardenCareActivity")]
        public int? GardenCareActivityId { get; set; }
        public virtual GardenCareActivity? GardenCareActivity { get; set; }

        [Display(Name = "Garden")]
        [ForeignKey("Garden")]
        public int? GardenId { get; set; }
        public virtual Garden? Garden { get; set; }

        // User relationship
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser? User { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Modified Date")]
        public DateTime? LastModifiedDate { get; set; }

        // Computed property for notification
        [NotMapped]
        public bool IsOverdue => !IsCompleted && ReminderDateTime < DateTime.UtcNow;

        [NotMapped]
        public bool IsDueToday => !IsCompleted && ReminderDateTime.Date == DateTime.UtcNow.Date;

        [NotMapped]
        public bool IsDueSoon => !IsCompleted && ReminderDateTime > DateTime.UtcNow && ReminderDateTime <= DateTime.UtcNow.AddHours(24);
    }
}