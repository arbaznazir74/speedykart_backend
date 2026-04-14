namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.AdminDashboard
{
    public class PlatformKpiSM
    {
        public decimal GmvThisMonth { get; set; }
        public decimal PlatformCommissionEarned { get; set; }
        public int TotalOrdersToday { get; set; }
        public int ActiveVendors { get; set; }
        public int NewVendorsThisWeek { get; set; }
        public int PendingRefundRequests { get; set; }
    }
}