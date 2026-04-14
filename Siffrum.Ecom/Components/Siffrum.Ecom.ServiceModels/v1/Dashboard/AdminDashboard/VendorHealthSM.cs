namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.AdminDashboard
{
    public class VendorHealthSM
    {
        public List<TopVendorSM> TopVendorsBySales { get; set; } = new();
        public List<TopVendorSM> VendorsWithHighRefundRate { get; set; } = new();
        public int DeactivatedSellers { get; set; }
    }
}