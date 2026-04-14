namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard
{
    public class SellerOrderSnapshotSM
    {
        public int Pending { get; set; }
        public int Processing { get; set; }
        public int Shipped { get; set; }
        public int Delivered { get; set; }
        public int Returned { get; set; }
    }
}
