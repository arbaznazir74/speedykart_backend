namespace CoreVisionServiceModels.v1.General.License
{
    public class SubscriptionUpgradeResponseSM
    {
        public string CurrentPlanName { get; set; }
        public decimal CurrentPlanPrice { get; set; }
        public string NewPlanName { get; set; }
        public decimal NewPlanPrice { get; set; }
        public decimal ProratedAmount { get; set; }
        public string ProrationCalculation { get; set; }
        public DateTime NextBillingDate { get; set; }
    }
}
