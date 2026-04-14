namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard
{
    public class SellerKpiSM
    {
        public decimal TodayRevenue { get; set; }
        public int OrdersToday { get; set; }
        public int PendingOrders { get; set; }
        public int LowStockCount { get; set; }
        public decimal PendingPayoutAmount { get; set; }
        public double StoreRating { get; set; }
    }
}
