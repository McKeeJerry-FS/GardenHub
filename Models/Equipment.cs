using GardenHub.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace GardenHub.Models
{
    public class Equipment
    {
        [Key]
        public int EquipmentId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Equipment name must be between 2 and 100 characters long.", MinimumLength = 2)]
        public string EquipmentName { get; set; } = string.Empty;

        [Required]
        [StringLength(500, ErrorMessage = "Equipment description must be between 2 and 500 characters long.", MinimumLength = 2)]
        public string EquipmentDescription { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Equipment Type")]
        public EquipmentType EquipmentType { get; set; }

        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; }

        [Display(Name = "Purchase Price")]
        public decimal PurchasePrice { get; set; }

        [Display(Name = "Last Maintenance Date")]
        public DateTime LastMaintenanceDate { get; set; }

        // Image Properties
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ImageType { get; set; }

        // Foreign Key to Garden
        [ForeignKey("Garden")]
        public int GardenId { get; set; }
        public Garden? Garden { get; set; }


        // Foreign key to AppUser

        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;

        public virtual AppUser? User { get; set; }
    }
}
