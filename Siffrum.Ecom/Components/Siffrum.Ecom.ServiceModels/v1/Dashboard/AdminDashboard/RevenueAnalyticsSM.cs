namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.AdminDashboard
{
    public class RevenueAnalyticsSM
    {        
        public List<DailyRevenueSM> CommissionTrend { get; set; } = new();
        public List<RefundRatioSM> RefundRatioOverlay { get; set; } = new();
    }
}