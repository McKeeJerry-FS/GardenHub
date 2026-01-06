using GardenHub.Models;
using GardenHub.Services.Interfaces;
using Stripe;

namespace GardenHub.Services
{
    public class StripeService : IStripeService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // Set Stripe API key
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public async Task<Customer> CreateCustomerAsync(AppUser user)
        {
            try
            {
                var options = new CustomerCreateOptions
                {
                    Email = user.Email,
                    Name = user.FullName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "app_user_id", user.Id }
                    }
                };

                var service = new CustomerService();
                var customer = await service.CreateAsync(options);
                
                _logger.LogInformation("Created Stripe customer {CustomerId} for user {UserId}", customer.Id, user.Id);
                return customer;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create Stripe customer for user {UserId}", user.Id);
                throw;
            }
        }

        public async Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId)
        {
            try
            {
                var options = new SubscriptionCreateOptions
                {
                    Customer = customerId,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = priceId
                        }
                    },
                    PaymentBehavior = "default_incomplete",
                    PaymentSettings = new SubscriptionPaymentSettingsOptions
                    {
                        SaveDefaultPaymentMethod = "on_subscription"
                    },
                    Expand = new List<string> { "latest_invoice.payment_intent" }
                };

                var service = new Stripe.SubscriptionService();
                var subscription = await service.CreateAsync(options);
                
                _logger.LogInformation("Created Stripe subscription {SubscriptionId} for customer {CustomerId}", 
                    subscription.Id, customerId);
                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create subscription for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Subscription> CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false)
        {
            try
            {
                var service = new Stripe.SubscriptionService();
                
                if (cancelImmediately)
                {
                    var subscription = await service.CancelAsync(subscriptionId);
                    _logger.LogInformation("Immediately cancelled subscription {SubscriptionId}", subscriptionId);
                    return subscription;
                }
                else
                {
                    var options = new SubscriptionUpdateOptions
                    {
                        CancelAtPeriodEnd = true
                    };
                    var subscription = await service.UpdateAsync(subscriptionId, options);
                    _logger.LogInformation("Scheduled subscription {SubscriptionId} for cancellation at period end", subscriptionId);
                    return subscription;
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to cancel subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<Subscription> ReactivateSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var options = new SubscriptionUpdateOptions
                {
                    CancelAtPeriodEnd = false
                };

                var service = new Stripe.SubscriptionService();
                var subscription = await service.UpdateAsync(subscriptionId, options);
                
                _logger.LogInformation("Reactivated subscription {SubscriptionId}", subscriptionId);
                return subscription;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to reactivate subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, Dictionary<string, string>? metadata = null)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = currency.ToLower(),
                    Customer = customerId,
                    AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    {
                        Enabled = true
                    },
                    Metadata = metadata
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                
                _logger.LogInformation("Created payment intent {PaymentIntentId} for customer {CustomerId}", 
                    paymentIntent.Id, customerId);
                return paymentIntent;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to create payment intent for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<Subscription?> GetSubscriptionAsync(string subscriptionId)
        {
            try
            {
                var service = new Stripe.SubscriptionService();
                return await service.GetAsync(subscriptionId);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to get subscription {SubscriptionId}", subscriptionId);
                return null;
            }
        }

        public async Task<Customer?> GetCustomerAsync(string customerId)
        {
            try
            {
                var service = new CustomerService();
                return await service.GetAsync(customerId);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to get customer {CustomerId}", customerId);
                return null;
            }
        }

        public async Task<bool> UpdatePaymentMethodAsync(string customerId, string paymentMethodId)
        {
            try
            {
                var options = new CustomerUpdateOptions
                {
                    InvoiceSettings = new CustomerInvoiceSettingsOptions
                    {
                        DefaultPaymentMethod = paymentMethodId
                    }
                };

                var service = new CustomerService();
                await service.UpdateAsync(customerId, options);
                
                _logger.LogInformation("Updated payment method for customer {CustomerId}", customerId);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to update payment method for customer {CustomerId}", customerId);
                return false;
            }
        }

        public async Task<List<Invoice>> GetCustomerInvoicesAsync(string customerId, int limit = 10)
        {
            try
            {
                var options = new InvoiceListOptions
                {
                    Customer = customerId,
                    Limit = limit
                };

                var service = new InvoiceService();
                var invoices = await service.ListAsync(options);
                return invoices.Data.ToList();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to get invoices for customer {CustomerId}", customerId);
                return new List<Invoice>();
            }
        }
    }
}