using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class OTPRegistrationSM
    {
        public string? SubscriptionId { get; set; } //FCM Token changed to player Id
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public int OTP { get; set; }
    }
}
