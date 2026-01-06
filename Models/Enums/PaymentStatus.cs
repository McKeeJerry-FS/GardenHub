namespace GardenHub.Models.Enums
{
    public enum PaymentStatus
    {
        Pending = 0,
        Succeeded = 1,
        Failed = 2,
        Refunded = 3,
        Cancelled = 4,
        RequiresAction = 5,
        Processing = 6,
    }
}
