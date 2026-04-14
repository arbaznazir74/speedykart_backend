namespace Siffrum.Ecom.ServiceModels.v1
{
    public class FinalSellerAssignedRequestSM
    {
        public long SellerId { get; set; }
        public long? StoreId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public long? AddressId { get; set; }
    }

    public class AssignedSellerResponseSM
    {
        public long? SellerId { get; set; }
        public long? StoreId { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
}
