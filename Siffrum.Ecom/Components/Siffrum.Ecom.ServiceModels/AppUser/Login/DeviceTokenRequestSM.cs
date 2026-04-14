using Siffrum.Ecom.ServiceModels.Enums;

namespace Siffrum.Ecom.ServiceModels.AppUser.Login
{
    public class DeviceTokenRequestSM
    {
        public long UserId { get; set; }
        public string PlayerId { get; set; }
        public DeviceTypeSM DeviceType { get; set; }
    }

}
