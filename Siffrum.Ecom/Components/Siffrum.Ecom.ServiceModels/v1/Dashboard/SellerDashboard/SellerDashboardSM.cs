namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard
{
    public class SellerDashboardSM
    {
        public SellerKpiSM Kpis { get; set; }
        public SellerSalesGraphSM SalesGraph { get; set; }
        public SellerOrderSnapshotSM Orders { get; set; }
        public SellerProductSnapshotSM Products { get; set; }
        public SellerFinancialSnapshotSM Financial { get; set; }
        public SellerCustomerInsightsSM Customers { get; set; }
    }
}
