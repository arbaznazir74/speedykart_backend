using CoreVisionServiceModels.AppUser.Login;
using CoreVisionServiceModels.Enums;
using CoreVisionServiceModels.Foundation.Base;

namespace CoreVisionServiceModels.AppUser
{
    public class ExternalUserSM : CoreVisionServiceModelBase<int>
    {
        public string RefreshToken { get; set; }
        public int ClientUserId { get; set; }

        public ExternalUserTypeSM ExternalUserType { get; set; }
    }
}
