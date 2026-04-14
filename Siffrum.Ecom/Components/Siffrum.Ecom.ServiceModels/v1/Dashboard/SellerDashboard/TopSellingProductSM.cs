namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard
{
    public class TopSellingProductSM
    {
        public long ProductVariantId { get; set; }
        public string Name { get; set; }
        public int QuantitySold { get; set; }
    }
}
