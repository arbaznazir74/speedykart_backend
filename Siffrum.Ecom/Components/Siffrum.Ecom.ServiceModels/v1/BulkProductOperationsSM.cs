using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class BulkStatusUpdateSM
    {
        public List<long> Ids { get; set; } = new();
        public ProductStatusSM Status { get; set; }
    }

    public class BulkAssignToSellersSM
    {
        public long ProductVariantId { get; set; }
        public List<long> SellerIds { get; set; } = new();
    }

    public class BulkPriceUpdateSM
    {
        public long ProductVariantId { get; set; }
        public double? Price { get; set; }
        public double? DiscountedPrice { get; set; }
        public long? SellerId { get; set; }
    }
}
