namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard
{
    public class SellerProductSnapshotSM
    {
        public int TotalActiveProducts { get; set; }
        public int OutOfStock { get; set; }
        public int RejectedProducts { get; set; }
        public List<TopSellingProductSM> TopSellingProducts { get; set; } = new();
    }

}
