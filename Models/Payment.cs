using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GardenHub.Models.Enums;

namespace GardenHub.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string AppUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(AppUserId))]
        public virtual AppUser? AppUser { get; set; }

        [Required]
        [StringLength(100)]
        public string StripePaymentIntentId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? StripeChargeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "USD";

        [Required]
        public PaymentStatus Status { get; set; }

        [Required]
        public UserTier TierAtPayment { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? FailureReason { get; set; }

        public DateTime? RefundedDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? RefundedAmount { get; set; }

        // Subscription information at time of payment
        public DateTime? BillingPeriodStart { get; set; }
        public DateTime? BillingPeriodEnd { get; set; }

        [StringLength(200)]
        public string? InvoiceUrl { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}