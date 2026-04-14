namespace Siffrum.Ecom.DomainModels.Enums
{
    public enum ProductStatusDM
    {
        Draft = 0,        // Created but not visible anywhere
        PendingApproval = 1, // Waiting for admin approval
        Active = 2,       // Live & purchasable
        Inactive = 3,     // Temporarily hidden by seller/admin
        Rejected = 4,     // Rejected by admin
        Blocked = 5,   // Policy violation / banned
        OutOfStock = 6    // Out of stock
    }
}
