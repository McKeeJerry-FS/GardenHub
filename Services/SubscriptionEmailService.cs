using GardenHub.Models;
using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text;

namespace GardenHub.Services
{
    public class SubscriptionEmailService : ISubscriptionEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<SubscriptionEmailService> _logger;
        private readonly IConfiguration _configuration;

        public SubscriptionEmailService(
            IEmailSender emailSender,
            ILogger<SubscriptionEmailService> logger,
            IConfiguration configuration)
        {
            _emailSender = emailSender;
            _logger = logger;
            _configuration = configuration;
        }

        #region Trial Emails

        public async Task SendTrialStartedEmailAsync(AppUser user)
        {
            var subject = "🌱 Welcome to GardenHub Pro Trial!";
            var body = BuildEmailTemplate(
                user,
                "Your Pro Trial Has Started!",
                $@"
                <p>Welcome to GardenHub Pro, {user.UserName}! 🎉</p>
                <p>Your 14-day free trial has started. You now have access to all Pro features including:</p>
                <ul>
                    <li>✅ Smart Reminders</li>
                    <li>✅ Advanced Analytics</li>
                    <li>✅ Data Export</li>
                    <li>✅ Priority Support</li>
                    <li>✅ Unlimited Gardens</li>
                </ul>
                <p><strong>Trial Period:</strong> {user.TrialStartDate:MMMM dd, yyyy} - {user.TrialEndDate:MMMM dd, yyyy}</p>
                <p>No credit card required during trial. We'll send you a reminder before your trial ends.</p>
                ",
                "Explore Pro Features",
                $"{GetBaseUrl()}/Home/Dashboard"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "TrialStarted");
        }

        public async Task SendTrialEndingReminderEmailAsync(AppUser user, int daysRemaining)
        {
            var subject = $"⏰ Your GardenHub Pro Trial Ends in {daysRemaining} Days";
            var body = BuildEmailTemplate(
                user,
                $"Your Trial Ends in {daysRemaining} Days",
                $@"
                <p>Hi {user.UserName},</p>
                <p>Your GardenHub Pro trial will end in <strong>{daysRemaining} days</strong> on {user.TrialEndDate:MMMM dd, yyyy}.</p>
                <p>Don't lose access to these amazing features:</p>
                <ul>
                    <li>📊 Advanced Analytics & Insights</li>
                    <li>⏰ Automated Care Reminders</li>
                    <li>📥 Data Export Tools</li>
                    <li>🎯 Priority Support</li>
                </ul>
                <p>Subscribe now to continue enjoying Pro benefits at just <strong>$9.99/month</strong>.</p>
                ",
                "Upgrade to Pro",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "TrialEndingReminder");
        }

        public async Task SendTrialExpiredEmailAsync(AppUser user)
        {
            var subject = "Your GardenHub Pro Trial Has Ended";
            var body = BuildEmailTemplate(
                user,
                "Your Trial Has Ended",
                $@"
                <p>Hi {user.UserName},</p>
                <p>Your 14-day Pro trial has ended. You've been moved back to the Hobby tier.</p>
                <p><strong>You still have access to:</strong></p>
                <ul>
                    <li>✅ Core Garden Management</li>
                    <li>✅ Plant Tracking</li>
                    <li>✅ Basic Analytics</li>
                    <li>✅ Garden Journal</li>
                </ul>
                <p>Ready to unlock Pro features again? Upgrade anytime for just $9.99/month.</p>
                ",
                "Upgrade to Pro",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "TrialExpired");
        }

        #endregion

        #region Subscription Lifecycle Emails

        public async Task SendSubscriptionActivatedEmailAsync(AppUser user)
        {
            var subject = "🎉 Welcome to GardenHub Pro!";
            var body = BuildEmailTemplate(
                user,
                "Your Pro Subscription is Active!",
                $@"
                <p>Congratulations, {user.UserName}! 🌟</p>
                <p>Your GardenHub Pro subscription is now active. Thank you for supporting GardenHub!</p>
                <p><strong>Subscription Details:</strong></p>
                <ul>
                    <li>Plan: GardenHub Pro</li>
                    <li>Price: $9.99/month</li>
                    <li>Next Billing Date: {user.NextBillingDate:MMMM dd, yyyy}</li>
                </ul>
                <p>You now have access to all Pro features. Start exploring!</p>
                ",
                "Go to Dashboard",
                $"{GetBaseUrl()}/Home/Dashboard"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "SubscriptionActivated");
        }

        public async Task SendSubscriptionCancelledEmailAsync(AppUser user)
        {
            var subject = "Your GardenHub Pro Subscription Has Been Cancelled";
            var body = BuildEmailTemplate(
                user,
                "Subscription Cancelled",
                $@"
                <p>Hi {user.UserName},</p>
                <p>We're sorry to see you go! Your Pro subscription has been cancelled.</p>
                <p><strong>Important Information:</strong></p>
                <ul>
                    <li>✅ Your Pro features remain active until {user.ProTierEndDate:MMMM dd, yyyy}</li>
                    <li>✅ No further charges will be made</li>
                    <li>✅ You can reactivate anytime before {user.GracePeriodEndDate:MMMM dd, yyyy}</li>
                </ul>
                <p>Changed your mind? You can reactivate your subscription from your account settings.</p>
                ",
                "Reactivate Subscription",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "SubscriptionCancelled");
        }

        public async Task SendSubscriptionReactivatedEmailAsync(AppUser user)
        {
            var subject = "🎊 Your GardenHub Pro Subscription is Reactivated!";
            var body = BuildEmailTemplate(
                user,
                "Welcome Back to Pro!",
                $@"
                <p>Great to have you back, {user.UserName}! 🎉</p>
                <p>Your Pro subscription has been successfully reactivated.</p>
                <p><strong>Subscription Details:</strong></p>
                <ul>
                    <li>Plan: GardenHub Pro</li>
                    <li>Price: $9.99/month</li>
                    <li>Next Billing Date: {user.NextBillingDate:MMMM dd, yyyy}</li>
                </ul>
                <p>Your Pro features are active and ready to use!</p>
                ",
                "Go to Dashboard",
                $"{GetBaseUrl()}/Home/Dashboard"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "SubscriptionReactivated");
        }

        #endregion

        #region Payment Emails

        public async Task SendPaymentSuccessEmailAsync(AppUser user, decimal amount, string currency)
        {
            var subject = "✅ Payment Received - GardenHub Pro";
            var body = BuildEmailTemplate(
                user,
                "Payment Successful",
                $@"
                <p>Hi {user.UserName},</p>
                <p>We've successfully processed your payment for GardenHub Pro.</p>
                <p><strong>Payment Details:</strong></p>
                <ul>
                    <li>Amount: {FormatCurrency(amount, currency)}</li>
                    <li>Date: {DateTime.UtcNow:MMMM dd, yyyy}</li>
                    <li>Next Billing Date: {user.NextBillingDate:MMMM dd, yyyy}</li>
                </ul>
                <p>Thank you for your continued support!</p>
                ",
                "View Invoice",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "PaymentSuccess");
        }

        public async Task SendPaymentFailedEmailAsync(AppUser user, string reason)
        {
            var subject = "⚠️ Payment Failed - Action Required";
            var body = BuildEmailTemplate(
                user,
                "Payment Failed",
                $@"
                <p>Hi {user.UserName},</p>
                <p>We were unable to process your payment for GardenHub Pro.</p>
                <p><strong>Reason:</strong> {reason}</p>
                <p><strong>What happens next:</strong></p>
                <ul>
                    <li>We'll retry your payment in 3 days</li>
                    <li>Your Pro features remain active during the grace period</li>
                    <li>Please update your payment method to avoid service interruption</li>
                </ul>
                <p>Update your payment method now to keep your Pro subscription active.</p>
                ",
                "Update Payment Method",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "PaymentFailed");
        }

        public async Task SendPaymentRetryNotificationEmailAsync(AppUser user, DateTime nextRetryDate)
        {
            var subject = "Payment Retry Scheduled - GardenHub Pro";
            var body = BuildEmailTemplate(
                user,
                "Payment Retry Scheduled",
                $@"
                <p>Hi {user.UserName},</p>
                <p>We'll retry processing your GardenHub Pro payment on <strong>{nextRetryDate:MMMM dd, yyyy}</strong>.</p>
                <p>To avoid service interruption, please ensure your payment method is up to date.</p>
                <p>If the payment fails again, your Pro subscription may be cancelled.</p>
                ",
                "Update Payment Method",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "PaymentRetry");
        }

        #endregion

        #region Grace Period & Warning Emails

        public async Task SendGracePeriodStartedEmailAsync(AppUser user, DateTime gracePeriodEnd)
        {
            var subject = "⏳ Grace Period Started - Update Payment Method";
            var body = BuildEmailTemplate(
                user,
                "Grace Period Started",
                $@"
                <p>Hi {user.UserName},</p>
                <p>Your payment failed, but don't worry - we've started a grace period.</p>
                <p><strong>Grace Period Details:</strong></p>
                <ul>
                    <li>Your Pro features remain active until {gracePeriodEnd:MMMM dd, yyyy}</li>
                    <li>You have {(gracePeriodEnd - DateTime.UtcNow).Days} days to update your payment method</li>
                    <li>After the grace period, your account will revert to the Hobby tier</li>
                </ul>
                <p>Please update your payment method to maintain your Pro subscription.</p>
                ",
                "Update Payment Method Now",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "GracePeriodStarted");
        }

        public async Task SendGracePeriodEndingEmailAsync(AppUser user, int daysRemaining)
        {
            var subject = $"🚨 Final Notice: Grace Period Ends in {daysRemaining} Days";
            var body = BuildEmailTemplate(
                user,
                "Final Warning: Grace Period Ending",
                $@"
                <p><strong>Hi {user.UserName},</strong></p>
                <p>Your grace period ends in <strong>{daysRemaining} days</strong> on {user.GracePeriodEndDate:MMMM dd, yyyy}.</p>
                <p><strong>⚠️ Action Required:</strong></p>
                <p>Update your payment method immediately to avoid losing access to Pro features:</p>
                <ul>
                    <li>📊 Advanced Analytics</li>
                    <li>⏰ Smart Reminders</li>
                    <li>📥 Data Export</li>
                    <li>🎯 Priority Support</li>
                </ul>
                <p>After the grace period, your account will be downgraded to the Hobby tier.</p>
                ",
                "Update Payment Method Now",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "GracePeriodEnding");
        }

        public async Task SendSubscriptionExpiredEmailAsync(AppUser user)
        {
            var subject = "Your GardenHub Pro Subscription Has Expired";
            var body = BuildEmailTemplate(
                user,
                "Subscription Expired",
                $@"
                <p>Hi {user.UserName},</p>
                <p>Your GardenHub Pro subscription has expired due to payment issues.</p>
                <p>You've been moved back to the Hobby tier, but you still have access to core features.</p>
                <p>Want to get back to Pro? Update your payment method and resubscribe anytime.</p>
                ",
                "Resubscribe to Pro",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "SubscriptionExpired");
        }

        #endregion

        #region Payment Reminders

        public async Task SendUpcomingPaymentReminderEmailAsync(AppUser user, DateTime paymentDate, decimal amount)
        {
            var subject = "Upcoming Payment Reminder - GardenHub Pro";
            var body = BuildEmailTemplate(
                user,
                "Upcoming Payment Reminder",
                $@"
                <p>Hi {user.UserName},</p>
                <p>This is a friendly reminder that your next GardenHub Pro payment is coming up.</p>
                <p><strong>Payment Details:</strong></p>
                <ul>
                    <li>Amount: {FormatCurrency(amount, "usd")}</li>
                    <li>Date: {paymentDate:MMMM dd, yyyy}</li>
                    <li>Subscription: GardenHub Pro (Monthly)</li>
                </ul>
                <p>Your payment method will be charged automatically on the date above.</p>
                ",
                "Manage Subscription",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "UpcomingPaymentReminder");
        }

        public async Task SendPaymentReceiptEmailAsync(AppUser user, string transactionId, decimal amount, string currency)
        {
            var subject = "Payment Receipt - GardenHub Pro";
            var body = BuildEmailTemplate(
                user,
                "Payment Receipt",
                $@"
                <p>Hi {user.UserName},</p>
                <p>Thank you for your payment! Here's your receipt.</p>
                <p><strong>Payment Details:</strong></p>
                <ul>
                    <li>Transaction ID: {transactionId}</li>
                    <li>Amount: {FormatCurrency(amount, currency)}</li>
                    <li>Date: {DateTime.UtcNow:MMMM dd, yyyy}</li>
                    <li>Description: GardenHub Pro - Monthly Subscription</li>
                    <li>Next Billing Date: {user.NextBillingDate:MMMM dd, yyyy}</li>
                </ul>
                <p>Keep this receipt for your records.</p>
                ",
                "View All Payments",
                $"{GetBaseUrl()}/Identity/Account/Manage/Subscription"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "PaymentReceipt");
        }

        #endregion

        #region Refund Emails

        public async Task SendRefundProcessedEmailAsync(AppUser user, decimal amount, string reason)
        {
            var subject = "Refund Processed - GardenHub";
            var body = BuildEmailTemplate(
                user,
                "Refund Processed",
                $@"
                <p>Hi {user.UserName},</p>
                <p>Your refund has been processed successfully.</p>
                <p><strong>Refund Details:</strong></p>
                <ul>
                    <li>Amount: {FormatCurrency(amount, "usd")}</li>
                    <li>Date: {DateTime.UtcNow:MMMM dd, yyyy}</li>
                    <li>Reason: {reason}</li>
                </ul>
                <p>The refund will appear in your account within 5-10 business days.</p>
                <p>If you have any questions, please contact our support team.</p>
                ",
                "Contact Support",
                $"{GetBaseUrl()}/Home/Contact"
            );

            await SendEmailWithLoggingAsync(user.Email, subject, body, "RefundProcessed");
        }

        #endregion

        #region Helper Methods

        private string BuildEmailTemplate(AppUser user, string heading, string content, string ctaText, string ctaUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ max-width: 600px; margin: 20px auto; background: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #56ab2f 0%, #a8e063 100%); color: white; padding: 40px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 28px; }}
        .content {{ padding: 30px; }}
        .content h2 {{ color: #56ab2f; margin-top: 0; }}
        .content ul {{ padding-left: 20px; }}
        .content ul li {{ margin: 10px 0; }}
        .cta-button {{ display: inline-block; padding: 15px 30px; background: #56ab2f; color: white !important; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
        .cta-button:hover {{ background: #4a9628; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; color: #6c757d; font-size: 12px; }}
        .footer a {{ color: #56ab2f; text-decoration: none; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🌱 GardenHub</h1>
        </div>
        <div class='content'>
            <h2>{heading}</h2>
            {content}
            <div style='text-align: center;'>
                <a href='{ctaUrl}' class='cta-button'>{ctaText}</a>
            </div>
        </div>
        <div class='footer'>
            <p>
                You're receiving this email because you have a GardenHub account.<br>
                <a href='{GetBaseUrl()}/Identity/Account/Manage/Subscription'>Manage Subscription</a> | 
                <a href='{GetBaseUrl()}/Home/Contact'>Contact Support</a> | 
                <a href='{GetBaseUrl()}/Home/Privacy'>Privacy Policy</a>
            </p>
            <p>© {DateTime.UtcNow.Year} GardenHub. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetBaseUrl()
        {
            return _configuration["AppSettings:BaseUrl"] ?? "https://localhost:7097";
        }

        private string FormatCurrency(decimal amount, string currency)
        {
            return currency.ToUpper() switch
            {
                "USD" => $"${amount:F2}",
                "EUR" => $"€{amount:F2}",
                "GBP" => $"£{amount:F2}",
                _ => $"{amount:F2} {currency.ToUpper()}"
            };
        }

        private async Task SendEmailWithLoggingAsync(string? email, string subject, string body, string emailType)
        {
            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Cannot send {EmailType} email: Email address is null or empty", emailType);
                return;
            }

            try
            {
                await _emailSender.SendEmailAsync(email, subject, body);
                _logger.LogInformation("Sent {EmailType} email to {Email}", emailType, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send {EmailType} email to {Email}", emailType, email);
                // Don't throw - we don't want email failures to break the subscription flow
            }
        }

        #endregion
    }
}