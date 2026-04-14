using CoreVisionServiceModels.Foundation.Base.CommonResponseRoot;
using CoreVisionServiceModels.Foundation.Base.Enums;
using CoreVisionServiceModels.Foundation.Base;
using CoreVisionServiceModels.Enums;

namespace CoreVisionServiceModels.AppUser.Login
{
    public class LoginUserSM : CoreVisionServiceModelBase<int>
    {
        public LoginUserSM()
        {
        }
        public RoleTypeSM RoleType { get; set; }
        public string LoginId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }

        [IgnorePropertyOnWrite(AutoMapConversionType.Dm2SmOnly)]
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public string ProfilePicturePath { get; set; }
        public bool IsPasswordPresent { get; set; } = false;
        public bool IsPhoneNumberConfirmed { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public LoginStatusSM LoginStatus { get; set; }
    }
}
