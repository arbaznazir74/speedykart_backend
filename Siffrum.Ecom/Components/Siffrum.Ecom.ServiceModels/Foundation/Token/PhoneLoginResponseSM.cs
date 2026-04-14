using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.Foundation.Token
{
    public class PhoneLoginResponseSM
    {
        public string CountryCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FCMToken { get; set; }
        public RoleTypeSM RoleType { get; set; }
    }
}
