using GardenHub.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Stripe;
using Stripe.Checkout;

namespace GardenHub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly ILogger<StripeWebhookController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPaymentService _paymentService;
        private readonly string _webhookSecret;

        public StripeWebhookController(
            ILogger<StripeWebhookController> logger,
            IConfiguration configuration,
            ISubscriptionService subscriptionService,
            IPaymentService paymentService)
        {
            _logger = logger;
            _configuration = configuration;
            _subscriptionService = subscriptionService;
            _paymentService = paymentService;
            _webhookSecret = _configuration["Stripe:WebhookSecret"] 
                ?? throw new InvalidOperationException("Stripe webhook secret not configured");
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"].ToString();

            try
            {
                // Verify webhook signature
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    signatureHeader,
                    _webhookSecret,
                    throwOnApiVersionMismatch: false
                );

                _logger.LogInformation("Received Stripe webhook: {EventType} - {EventId}", 
                    stripeEvent.Type, stripeEvent.Id);

                // Handle the event
                switch (stripeEvent.Type)
                {
                    // Checkout Session Events
                        case "checkout.session.completed":
                        await HandleCheckoutSessionCompleted(stripeEvent);
                        break;
                    
                    case "checkout.session.expired":
                        await HandleCheckoutSessionExpired(stripeEvent);
                        break;

                    // Payment Intent Events
                    case "payment_intent.succeeded":
                        await HandlePaymentIntentSucceeded(stripeEvent);
                        break;

                    case "payment_intent.payment_failed":
                        await HandlePaymentIntentFailed(stripeEvent);
                        break;

                    case "payment_intent.canceled":
                        await HandlePaymentIntentCanceled(stripeEvent);
                        break;

                    // Subscription Events
                    case "customer.subscription.created":
                        await HandleSubscriptionCreated(stripeEvent);
                        break;

                    case "customer.subscription.updated":
                        await HandleSubscriptionUpdated(stripeEvent);
                        break;

                    case "customer.subscription.deleted":
                        await HandleSubscriptionDeleted(stripeEvent);
                        break;

                    case "customer.subscription.trial_will_end":
                        await HandleSubscriptionTrialWillEnd(stripeEvent);
                        break;

                    // Invoice Events
                    case "invoice.payment_succeeded":
                        await HandleInvoicePaymentSucceeded(stripeEvent);
                        break;

                    case "invoice.payment_failed":
                        await HandleInvoicePaymentFailed(stripeEvent);
                        break;

                    case "invoice.finalized":
                        await HandleInvoiceFinalized(stripeEvent);
                        break;

                    // Charge Events
                    case "charge.succeeded":
                        await HandleChargeSucceeded(stripeEvent);
                        break;

                    case "charge.failed":
                        await HandleChargeFailed(stripeEvent);
                        break;

                    case "charge.refunded":
                        await HandleChargeRefunded(stripeEvent);
                        break;

                    // Customer Events
                    case "customer.created":
                        await HandleCustomerCreated(stripeEvent);
                        break;

                    case "customer.updated":
                        await HandleCustomerUpdated(stripeEvent);
                        break;

                    case "customer.deleted":
                        await HandleCustomerDeleted(stripeEvent);
                        break;

                    default:
                        _logger.LogInformation("Unhandled webhook event type: {EventType}", stripeEvent.Type);
                        break;
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook error: {Message}", ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing webhook");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        #region Checkout Session Handlers

        private async Task HandleCheckoutSessionCompleted(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;

            _logger.LogInformation("Checkout session completed: {SessionId} for customer {CustomerId}", 
                session.Id, session.CustomerId);

            try
            {
                var userId = session.Metadata?["userId"];
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("No userId in session metadata for session {SessionId}", session.Id);
                    return;
                }

                // Activate subscription
                if (!string.IsNullOrEmpty(session.SubscriptionId))
                {
                    await _subscriptionService.ActivateSubscriptionAsync(
                        userId, 
                        session.SubscriptionId, 
                        session.CustomerId);

                    _logger.LogInformation("Activated subscription {SubscriptionId} for user {UserId}", 
                        session.SubscriptionId, userId);
                }

                // Record the payment
                if (session.AmountTotal.HasValue)
                {
                    await _paymentService.RecordSuccessfulPaymentAsync(
                        userId,
                        session.Id,
                        session.AmountTotal.Value / 100m, // Convert from cents
                        session.Currency ?? "usd",
                        session.PaymentIntentId,
                        session.CustomerId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling checkout session completed: {SessionId}", session.Id);
                throw;
            }
        }

        private async Task HandleCheckoutSessionExpired(Event stripeEvent)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;

            _logger.LogInformation("Checkout session expired: {SessionId}", session.Id);

            var userId = session.Metadata?["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                await _paymentService.RecordFailedPaymentAsync(
                    userId,
                    session.Id,
                    "Checkout session expired");
            }
        }

        #endregion

        #region Payment Intent Handlers

        private async Task HandlePaymentIntentSucceeded(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogInformation("Payment succeeded: {PaymentIntentId} - Amount: {Amount}", 
                paymentIntent.Id, paymentIntent.Amount);

            var userId = paymentIntent.Metadata?["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                await _paymentService.UpdatePaymentStatusAsync(
                    paymentIntent.Id,
                    Models.Enums.PaymentStatus.Succeeded);
            }
        }

        private async Task HandlePaymentIntentFailed(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogWarning("Payment failed: {PaymentIntentId} - Reason: {Reason}", 
                paymentIntent.Id, paymentIntent.LastPaymentError?.Message);

            var userId = paymentIntent.Metadata?["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                await _paymentService.RecordFailedPaymentAsync(
                    userId,
                    paymentIntent.Id,
                    paymentIntent.LastPaymentError?.Message ?? "Payment failed");

                // Handle failed subscription payment
                await _subscriptionService.HandleFailedPaymentAsync(userId);
            }
        }

        private async Task HandlePaymentIntentCanceled(Event stripeEvent)
        {
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogInformation("Payment canceled: {PaymentIntentId}", paymentIntent.Id);

            await _paymentService.UpdatePaymentStatusAsync(
                paymentIntent.Id,
                Models.Enums.PaymentStatus.Cancelled);
        }

        #endregion

        #region Subscription Handlers

        private async Task HandleSubscriptionCreated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation("Subscription created: {SubscriptionId} for customer {CustomerId}", 
                subscription.Id, subscription.CustomerId);

            var userId = subscription.Metadata?["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                DateTime? currentPeriodEnd = null;
#if NET6_0_OR_GREATER
                if (subscription.RawJObject.TryGetValue("current_period_end", out var periodEndToken))
                {
                    var unix = periodEndToken.Value<long?>();
                    if (unix.HasValue)
                        currentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds(unix.Value).UtcDateTime;
                }
#else
                var unix = subscription.RawJObject?["current_period_end"]?.Value<long?>();
                if (unix.HasValue)
                    currentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds(unix.Value).UtcDateTime;
#endif

                if (currentPeriodEnd.HasValue)
                {
                    await _subscriptionService.CreateSubscriptionRecordAsync(
                        userId,
                        subscription.Id,
                        subscription.CustomerId,
                        subscription.Status,
                        currentPeriodEnd.Value);
                }
                else
                {
                    _logger.LogWarning("currentPeriodEnd is null for subscription {SubscriptionId}", subscription.Id);
                }
            }
        }

        private async Task HandleSubscriptionUpdated(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation("Subscription updated: {SubscriptionId} - Status: {Status}", 
                subscription.Id, subscription.Status);

            DateTime? currentPeriodEnd = null;
#if NET6_0_OR_GREATER
            if (subscription.RawJObject.TryGetValue("current_period_end", out var periodEndToken))
            {
                var unix = periodEndToken.Value<long?>();
                if (unix.HasValue)
                    currentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds(unix.Value).UtcDateTime;
            }
#else
            var unix = subscription.RawJObject?["current_period_end"]?.Value<long?>();
            if (unix.HasValue)
                currentPeriodEnd = DateTimeOffset.FromUnixTimeSeconds(unix.Value).UtcDateTime;
#endif

            if (currentPeriodEnd.HasValue)
            {
                await _subscriptionService.UpdateSubscriptionStatusAsync(
                    subscription.Id,
                    subscription.Status,
                    currentPeriodEnd.Value);
            }
            else
            {
                _logger.LogWarning("currentPeriodEnd is null for subscription {SubscriptionId}", subscription.Id);
            }

            // Handle subscription cancellation
            if (subscription.CancelAtPeriodEnd)
            {
                _logger.LogInformation("Subscription {SubscriptionId} will cancel at period end: {EndDate}", 
                    subscription.Id, currentPeriodEnd);

                if (currentPeriodEnd.HasValue)
                {
                    await _subscriptionService.MarkSubscriptionForCancellationAsync(
                        subscription.Id,
                        currentPeriodEnd.Value);
                }
                else
                {
                    _logger.LogWarning("currentPeriodEnd is null for cancellation of subscription {SubscriptionId}", subscription.Id);
                }
            }
        }

        private async Task HandleSubscriptionDeleted(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation("Subscription deleted: {SubscriptionId}", subscription.Id);

            await _subscriptionService.DeactivateSubscriptionAsync(subscription.Id);
        }

        private async Task HandleSubscriptionTrialWillEnd(Event stripeEvent)
        {
            var subscription = stripeEvent.Data.Object as Subscription;
            if (subscription == null) return;

            _logger.LogInformation("Subscription trial ending soon: {SubscriptionId} - Ends: {TrialEnd}", 
                subscription.Id, subscription.TrialEnd);

            var userId = subscription.Metadata?["userId"];
            if (!string.IsNullOrEmpty(userId) && subscription.TrialEnd.HasValue)
            {
                await _subscriptionService.NotifyTrialEndingAsync(userId, subscription.TrialEnd.Value);
            }
        }

        #endregion

        #region Invoice Handlers

        private async Task HandleInvoicePaymentSucceeded(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null) return;

            _logger.LogInformation("Invoice payment succeeded: {InvoiceId} - Amount: {Amount}", 
                invoice.Id, invoice.AmountPaid);

            // Get subscriptionId from RawJObject
            var subscriptionId = invoice.RawJObject?["subscription"]?.Value<string>();
            var userId = !string.IsNullOrEmpty(subscriptionId)
                ? await _subscriptionService.GetUserIdBySubscriptionIdAsync(subscriptionId)
                : null;

            if (!string.IsNullOrEmpty(userId))
            {
                await _paymentService.RecordSuccessfulPaymentAsync(
                    userId,
                    invoice.Id,
                    invoice.AmountPaid / 100m,
                    invoice.Currency ?? "usd",
                    null, // PaymentIntentId not available on Invoice
                    invoice.CustomerId);

                // Extend subscription period
                if (!string.IsNullOrEmpty(subscriptionId))
                {
                    await _subscriptionService.ExtendSubscriptionAsync(
                        subscriptionId,
                        invoice.PeriodEnd);
                }
            }
        }

        private async Task HandleInvoicePaymentFailed(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null) return;

            _logger.LogWarning("Invoice payment failed: {InvoiceId} - Attempt: {Attempt}", 
                invoice.Id, invoice.AttemptCount);

            // Get subscriptionId from RawJObject
            var subscriptionId = invoice.RawJObject?["subscription"]?.Value<string>();
            var userId = !string.IsNullOrEmpty(subscriptionId)
                ? await _subscriptionService.GetUserIdBySubscriptionIdAsync(subscriptionId)
                : null;

            if (!string.IsNullOrEmpty(userId))
            {
                await _paymentService.RecordFailedPaymentAsync(
                    userId,
                    invoice.Id,
                    $"Invoice payment failed - Attempt {invoice.AttemptCount}");

                // Start grace period after first failed payment
                if (invoice.AttemptCount == 1)
                {
                    await _subscriptionService.StartGracePeriodAsync(userId);
                }

                // Cancel subscription after multiple failed attempts
                if (invoice.AttemptCount >= 3 && !string.IsNullOrEmpty(subscriptionId))
                {
                    await _subscriptionService.CancelSubscriptionDueToFailedPaymentAsync(
                        subscriptionId);
                }
            }
        }

        private async Task HandleInvoiceFinalized(Event stripeEvent)
        {
            var invoice = stripeEvent.Data.Object as Invoice;
            if (invoice == null) return;

            _logger.LogInformation("Invoice finalized: {InvoiceId}", invoice.Id);

            // You can send invoice emails or notifications here
            await Task.CompletedTask;
        }

        #endregion

        #region Charge Handlers

        private async Task HandleChargeSucceeded(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null) return;

            _logger.LogInformation("Charge succeeded: {ChargeId} - Amount: {Amount}", 
                charge.Id, charge.Amount);

            if (!string.IsNullOrEmpty(charge.PaymentIntentId))
            {
                await _paymentService.UpdatePaymentStatusAsync(
                    charge.PaymentIntentId,
                    Models.Enums.PaymentStatus.Succeeded);
            }
        }

        private async Task HandleChargeFailed(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null) return;

            _logger.LogWarning("Charge failed: {ChargeId} - Reason: {Reason}", 
                charge.Id, charge.FailureMessage);

            if (!string.IsNullOrEmpty(charge.PaymentIntentId))
            {
                await _paymentService.UpdatePaymentStatusAsync(
                    charge.PaymentIntentId,
                    Models.Enums.PaymentStatus.Failed);
            }
        }

        private async Task HandleChargeRefunded(Event stripeEvent)
        {
            var charge = stripeEvent.Data.Object as Charge;
            if (charge == null) return;

            _logger.LogInformation("Charge refunded: {ChargeId} - Amount: {Amount}", 
                charge.Id, charge.AmountRefunded);

            if (!string.IsNullOrEmpty(charge.PaymentIntentId))
            {
                await _paymentService.RecordRefundAsync(
                    charge.PaymentIntentId,
                    charge.AmountRefunded / 100m,
                    charge.FailureMessage ?? "Refund processed");
            }
        }

        #endregion

        #region Customer Handlers

        private async Task HandleCustomerCreated(Event stripeEvent)
        {
            var customer = stripeEvent.Data.Object as Customer;
            if (customer == null) return;

            _logger.LogInformation("Customer created: {CustomerId} - Email: {Email}", 
                customer.Id, customer.Email);
            
            await Task.CompletedTask;
        }

        private async Task HandleCustomerUpdated(Event stripeEvent)
        {
            var customer = stripeEvent.Data.Object as Customer;
            if (customer == null) return;

            _logger.LogInformation("Customer updated: {CustomerId}", customer.Id);
            
            await Task.CompletedTask;
        }

        private async Task HandleCustomerDeleted(Event stripeEvent)
        {
            var customer = stripeEvent.Data.Object as Customer;
            if (customer == null) return;

            _logger.LogInformation("Customer deleted: {CustomerId}", customer.Id);

            var userId = customer.Metadata?["userId"];
            if (!string.IsNullOrEmpty(userId))
            {
                await _subscriptionService.RemoveCustomerReferenceAsync(userId);
            }
        }

        #endregion
    }
}