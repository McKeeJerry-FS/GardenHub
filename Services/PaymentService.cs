using GardenHub.Data;
using GardenHub.Models;
using GardenHub.Models.Enums;
using GardenHub.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GardenHub.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(ApplicationDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Payment> CreatePaymentRecordAsync(string userId, string paymentIntentId, decimal amount, UserTier tier, string description)
        {
            try
            {
                var payment = new Payment
                {
                    AppUserId = userId,
                    StripePaymentIntentId = paymentIntentId,
                    Amount = amount,
                    Currency = "USD",
                    Status = PaymentStatus.Pending,
                    TierAtPayment = tier,
                    Description = description,
                    PaymentDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created payment record {PaymentId} for user {UserId}", payment.Id, userId);
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment record for user {UserId}", userId);
                throw;
            }
        }

        public async Task<Payment?> GetPaymentByIntentIdAsync(string paymentIntentId)
        {
            return await _context.Payments
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);
        }

        public async Task<List<Payment>> GetUserPaymentsAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Payments
                .Where(p => p.AppUserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<bool> UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status, string? failureReason = null)
        {
            try
            {
                var payment = await GetPaymentByIntentIdAsync(paymentIntentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment with intent ID {PaymentIntentId} not found", paymentIntentId);
                    return false;
                }

                payment.Status = status;
                payment.FailureReason = failureReason;
                payment.UpdatedDate = DateTime.UtcNow;

                if (status == PaymentStatus.Succeeded)
                {
                    payment.PaymentDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated payment {PaymentId} status to {Status}", payment.Id, status);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment status for intent {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<bool> RecordRefundAsync(string paymentIntentId, decimal refundAmount)
        {
            try
            {
                var payment = await GetPaymentByIntentIdAsync(paymentIntentId);
                if (payment == null)
                {
                    return false;
                }

                payment.Status = PaymentStatus.Refunded;
                payment.RefundedAmount = refundAmount;
                payment.RefundedDate = DateTime.UtcNow;
                payment.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Recorded refund for payment {PaymentId}", payment.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record refund for intent {PaymentIntentId}", paymentIntentId);
                return false;
            }
        }

        public async Task<decimal> GetTotalPaidByUserAsync(string userId)
        {
            return await _context.Payments
                .Where(p => p.AppUserId == userId && p.Status == PaymentStatus.Succeeded)
                .SumAsync(p => p.Amount);
        }

        public async Task<Payment?> GetLatestSuccessfulPaymentAsync(string userId)
        {
            return await _context.Payments
                .Where(p => p.AppUserId == userId && p.Status == PaymentStatus.Succeeded)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefaultAsync();
        }
    }
}