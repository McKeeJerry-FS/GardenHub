using GardenHub.Models;
using GardenHub.Models.Enums;

namespace GardenHub.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> RecordSuccessfulPaymentAsync(
            string userId, 
            string transactionId, 
            decimal amount, 
            string currency,
            string? paymentIntentId = null,
            string? customerId = null);

        Task<Payment> RecordFailedPaymentAsync(
            string userId, 
            string transactionId, 
            string failureReason);

        Task UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status);
        
        Task<Payment> RecordRefundAsync(
            string paymentIntentId, 
            decimal refundAmount, 
            string reason);

        Task<List<Payment>> GetUserPaymentsAsync(string userId);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
    }
}