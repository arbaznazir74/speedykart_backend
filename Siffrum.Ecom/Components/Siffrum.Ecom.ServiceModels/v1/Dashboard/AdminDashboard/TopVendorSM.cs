namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.AdminDashboard
{
    public class TopVendorSM
    {
        public long SellerId { get; set; }
        public string? StoreName { get; set; }
        public decimal TotalSales { get; set; }
        public double RefundRate { get; set; }
    }
}