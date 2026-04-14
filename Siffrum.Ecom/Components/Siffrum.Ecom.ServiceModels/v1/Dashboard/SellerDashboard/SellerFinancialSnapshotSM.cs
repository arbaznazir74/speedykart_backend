namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard
{
    public class SellerFinancialSnapshotSM
    {
        public decimal AvailableBalance { get; set; }
        public decimal LockedBalance { get; set; }
        public decimal CommissionPaid { get; set; }
        public DateTime? UpcomingPayoutDate { get; set; }
    }
}
