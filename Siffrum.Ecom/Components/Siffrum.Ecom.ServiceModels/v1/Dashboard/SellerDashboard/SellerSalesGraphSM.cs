namespace Siffrum.Ecom.ServiceModels.v1.Dashboard.SellerDashboard
{
    public class SellerSalesGraphSM
    {
        public List<SalesGraphPointSM> Data { get; set; } = new();
        public decimal GrowthPercentage { get; set; }
    }
    public class SalesGraphPointSM
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }
}
