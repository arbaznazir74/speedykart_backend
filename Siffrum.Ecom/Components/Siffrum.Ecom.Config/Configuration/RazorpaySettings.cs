namespace Siffrum.Ecom.Config.Configuration
{
    public class RazorpaySettings
    {
        public string KeyId { get; set; }
        public string KeySecret { get; set; }
        public string WebhookSecret { get; set; }

        public string Signature { get; set; }
    }
}
