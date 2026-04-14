namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.AdminDashboard
{
    public class OrderHealthSM
    {
        public List<OrderStatusCountSM> OrdersByStatus { get; set; } = new();
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
    }
}