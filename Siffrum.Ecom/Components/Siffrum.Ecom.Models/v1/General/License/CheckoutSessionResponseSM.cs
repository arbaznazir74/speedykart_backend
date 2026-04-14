namespace CoreVisionServiceModels.v1.General.License
{
    public class CheckoutSessionResponseSM
    {
        public string SessionId { get; set; }
        public string PublicKey { get; set; }
        public string Url { get; set; }
        public bool IsNewSubscription { get; set; }
    }
}
