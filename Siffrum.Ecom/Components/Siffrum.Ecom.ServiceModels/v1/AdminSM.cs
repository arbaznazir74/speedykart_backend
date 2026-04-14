using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.v1
{
    public class AdminSM : SiffrumServiceModelBase<long>
    {
        public string? Username { get; set; }

        public string Email { get; set; } = null!;

        public string? Password { get; set; }

        public string? ForgotPasswordCode { get; set; }

        public string? FcmId { get; set; }

        public string? RememberToken { get; set; }

        public StatusSM Status { get; set; }

        public LoginStatusSM LoginStatus { get; set; }

        public DateTime? LoginAt { get; set; }

        public DateTime? LastActiveAt { get; set; }

        public RoleTypeSM RoleType { get; set; }


    }
}
