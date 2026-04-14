namespace Siffrum.Ecom.ServiceModels.Foundation.Token
{
    public class PhoneLoginRequest
    {
        public string FirebaseToken { get; set; }

        public string? DeviceToken { get; set; }
    }
}
