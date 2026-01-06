using GardenHub.Models;
using GardenHub.Models.Enums;

namespace GardenHub.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentRecordAsync(string userId, string paymentIntentId, decimal amount, UserTier tier, string description);
        Task<Payment?> GetPaymentByIntentIdAsync(string paymentIntentId);
        Task<List<Payment>> GetUserPaymentsAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task<bool> UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status, string? failureReason = null);
        Task<bool> RecordRefundAsync(string paymentIntentId, decimal refundAmount);
        Task<decimal> GetTotalPaidByUserAsync(string userId);
        Task<Payment?> GetLatestSuccessfulPaymentAsync(string userId);
    }
}