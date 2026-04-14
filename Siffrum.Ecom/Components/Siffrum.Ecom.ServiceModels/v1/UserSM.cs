using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class UserSM : SiffrumServiceModelBase<long>
    {
        public string? Name { get; set; }

        public string? Username { get; set; }

        public string? Email { get; set; }

        public string? Password { get; set; }

        public string? EmailVerificationCode { get; set; }

        public string? Image { get; set; }
        public string? NetworkImage { get; set; }

        public string? CountryCode { get; set; }

        public string? Mobile { get; set; }

        public double Balance { get; set; }

        public string? ReferralCode { get; set; }

        public string? FriendsCode { get; set; }

        public StatusSM Status { get; set; }

        public LoginStatusSM LoginStatus { get; set; }

        public DateTime? DeletedAt { get; set; }

        public string? PaymentId { get; set; }

        public string? PmType { get; set; }

        public string? PmLastFour { get; set; }

        public DateTime? TrialEndsAt { get; set; }

        public RoleTypeSM RoleType { get; set; }

        public DeviceTypeSM DeviceType { get; set; }

        public int OTP { get; set; }

        public string? FcmId { get; set; }
        public bool IsEmailConfirmed { get; set; }

        public bool IsMobileConfirmed { get; set; }



    }
}
