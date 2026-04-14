namespace Siffrum.Ecom.ServiceModels.v1
{
    public class SellerEarningsSM
    {
        public decimal TodayEarnings { get; set; }
        public decimal MonthEarnings { get; set; }
        public int TodayOrders { get; set; }
        public int MonthOrders { get; set; }
    }
}
