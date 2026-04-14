namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.AdminDashboard
{
    public class DailyRevenueSM
    {
        public DateTime Date { get; set; }
        public double Amount { get; set; }
        public int OrderCount { get; set; }
    }
}