namespace Siffrum.Ecom.DomainModels.Enums
{
    public enum PaymentStatusDM
    {
        Pending = 0,
        Paid = 1,
        Failed = 2,
        Refunded = 3,
        PartiallyRefunded = 4,
        Flagged = 5,
        Cancelled = 6,
        RefundInitiated = 7
    }

}
