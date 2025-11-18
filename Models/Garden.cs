using GardenHub.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardenHub.Models
{
    public class Garden
    {
        [Key]
        public int GardenId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Garden name must be between 2 and 100 characters long.", MinimumLength = 2)]
        public string GardenName { get; set; } = string.Empty;

        [Required]
        [StringLength(500, ErrorMessage = "Garden description must be between 2 and 500 characters long.", MinimumLength = 2)]
        public string GardenDescription { get; set; } = string.Empty;

        [Display(Name = "Garden Location")]
        public Location GardenLocation { get; set; }

        [Display(Name = "Garden Type")]
        public GardenType Type { get; set; }

        [Display(Name = "Grow Method")]
        public GrowMethod GardenGrowMethod { get; set; }
        
        [Required]
        [Display(Name = "Garden Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Garden End Date")]
        public DateTime EndDate { get; set; }


        // Foreign key to AppUser
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public virtual AppUser? User { get; set; }


        // Navigation properties
        public virtual ICollection<DailyRecord> DailyRecords { get; set; } = new List<DailyRecord>();
        public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
        public virtual ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
