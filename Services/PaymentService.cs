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

        // NEW: Record successful payment with all details
        public async Task<Payment> RecordSuccessfulPaymentAsync(
            string userId, 
            string transactionId, 
            decimal amount, 
            string currency,
            string? paymentIntentId = null,
            string? customerId = null)
        {
            try
            {
                var payment = new Payment
                {
                    AppUserId = userId,
                    StripePaymentIntentId = paymentIntentId ?? transactionId,
                    Amount = amount,
                    Currency = currency.ToUpperInvariant(),
                    Status = PaymentStatus.Succeeded,
                    Description = "Subscription payment",
                    PaymentDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Recorded successful payment {PaymentId} for user {UserId}, Amount: {Amount} {Currency}", 
                    payment.Id, userId, amount, currency);
                
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record successful payment for user {UserId}", userId);
                throw;
            }
        }

        // NEW: Record failed payment
        public async Task<Payment> RecordFailedPaymentAsync(
            string userId, 
            string transactionId, 
            string failureReason)
        {
            try
            {
                var payment = new Payment
                {
                    AppUserId = userId,
                    StripePaymentIntentId = transactionId,
                    Amount = 0, // Amount not available for failed payments
                    Currency = "USD",
                    Status = PaymentStatus.Failed,
                    FailureReason = failureReason,
                    Description = "Failed payment attempt",
                    PaymentDate = DateTime.UtcNow,
                    CreatedDate = DateTime.UtcNow,
                    UpdatedDate = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Recorded failed payment for user {UserId}: {Reason}", userId, failureReason);
                
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record failed payment for user {UserId}", userId);
                throw;
            }
        }

        // UPDATED: Simplified signature to match interface
        public async Task UpdatePaymentStatusAsync(string paymentIntentId, PaymentStatus status)
        {
            try
            {
                var payment = await GetPaymentByIntentIdAsync(paymentIntentId);
                if (payment == null)
                {
                    _logger.LogWarning("Payment with intent ID {PaymentIntentId} not found", paymentIntentId);
                    return;
                }

                payment.Status = status;
                payment.UpdatedDate = DateTime.UtcNow;

                if (status == PaymentStatus.Succeeded)
                {
                    payment.PaymentDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated payment {PaymentId} status to {Status}", payment.Id, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update payment status for intent {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

        // UPDATED: Return Payment and include reason parameter
        public async Task<Payment> RecordRefundAsync(
            string paymentIntentId, 
            decimal refundAmount, 
            string reason)
        {
            try
            {
                var payment = await GetPaymentByIntentIdAsync(paymentIntentId);
                if (payment == null)
                {
                    throw new InvalidOperationException($"Payment with intent ID {paymentIntentId} not found");
                }

                payment.Status = PaymentStatus.Refunded;
                payment.RefundedAmount = refundAmount;
                payment.RefundedDate = DateTime.UtcNow;
                payment.FailureReason = reason; // Store refund reason
                payment.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Recorded refund of {Amount} for payment {PaymentId}: {Reason}", 
                    refundAmount, payment.Id, reason);
                
                return payment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to record refund for intent {PaymentIntentId}", paymentIntentId);
                throw;
            }
        }

        // UPDATED: Renamed and simplified to match interface
        public async Task<List<Payment>> GetUserPaymentsAsync(string userId)
        {
            return await _context.Payments
                .Where(p => p.AppUserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        // NEW: Get payment by ID
        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        // NEW: Get payment by transaction ID (alias for GetPaymentByIntentIdAsync)
        public async Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId)
        {
            return await GetPaymentByIntentIdAsync(transactionId);
        }

        // EXISTING: Keep for backward compatibility
        public async Task<Payment> CreatePaymentRecordAsync(
            string userId, 
            string paymentIntentId, 
            decimal amount, 
            UserTier tier, 
            string description)
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

        // EXISTING: Keep internal method
        private async Task<Payment?> GetPaymentByIntentIdAsync(string paymentIntentId)
        {
            return await _context.Payments
                .Include(p => p.AppUser)
                .FirstOrDefaultAsync(p => p.StripePaymentIntentId == paymentIntentId);
        }

        // EXISTING: Keep for backward compatibility
        public async Task<bool> UpdatePaymentStatusAsync(
            string paymentIntentId, 
            PaymentStatus status, 
            string? failureReason = null)
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

        // EXISTING: Keep utility methods
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