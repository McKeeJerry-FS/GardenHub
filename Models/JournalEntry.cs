using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GardenHub.Models
{
    public class JournalEntry
    {
        [Key]
        public int EntryId { get; set; }

        [ForeignKey("Garden")]
        public int GardenId { get; set; }

        
        public Garden Garden { get; set; } = null!;

        [Required]
        [Display(Name = "Entry Date")]
        public DateTime EntryDate { get; set; }

        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

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
