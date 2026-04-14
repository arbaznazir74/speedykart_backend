namespace CoreVisionServiceModels.v1.General.License
{
    public class SubscriptionUpgradeRequestSM
    {
        public string? StripeCustomerId { get; set; }
        public string StripeSubscriptionId { get; set; }
        public string NewStripePriceId { get; set; }
    }
}
