using GardenHub.Models;
using Stripe;

namespace GardenHub.Services.Interfaces
{
    public interface IStripeService
    {
        Task<Customer> CreateCustomerAsync(AppUser user);
        Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId);
        Task<Subscription> CancelSubscriptionAsync(string subscriptionId, bool cancelImmediately = false);
        Task<Subscription> ReactivateSubscriptionAsync(string subscriptionId);
        Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, Dictionary<string, string>? metadata = null);
        Task<Subscription?> GetSubscriptionAsync(string subscriptionId);
        Task<Customer?> GetCustomerAsync(string customerId);
        Task<bool> UpdatePaymentMethodAsync(string customerId, string paymentMethodId);
        Task<List<Invoice>> GetCustomerInvoicesAsync(string customerId, int limit = 10);
    }
}