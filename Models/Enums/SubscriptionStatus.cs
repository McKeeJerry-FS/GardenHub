namespace GardenHub.Models.Enums
{
    public enum SubscriptionStatus
    {
        None = 0,           // Never subscribed (Hobby tier)
        Trialing = 1,       // On free trial period
        Active = 2,         // Currently paying and active
        Cancelled = 3,      // Cancelled but in 14-day grace period
        Expired = 4,        // Grace period ended, reverted to Hobby
        PastDue = 5,        // Payment failed, needs attention
        Suspended = 6       // Manually suspended by admin
    }
}
