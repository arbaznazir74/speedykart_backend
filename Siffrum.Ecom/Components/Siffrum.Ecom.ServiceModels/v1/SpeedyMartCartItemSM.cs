namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SpeedyMartCartItemSM
    {
        public long Id { get; set; }
        public long CartId { get; set; }
        public long ProductVariantId { get; set; }
        public UserSpeedyMartProductSM? SpeedyMartProductDetails { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
