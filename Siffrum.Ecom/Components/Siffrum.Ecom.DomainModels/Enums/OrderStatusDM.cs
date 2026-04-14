namespace Siffrum.Ecom.DomainModels.Enums
{
    public enum OrderStatusDM
    {
        Created = 0,
        Processing = 1,
        Shipped = 2,
        Delivered = 3,
        Cancelled = 4,
        Returned = 5,
        Failed = 6,
        Assigned = 7,
        CancelledBySeller = 8,
        SellerAccepted = 9,
        PickedUp = 10,
        OutForDelivery = 11,
    }
}
