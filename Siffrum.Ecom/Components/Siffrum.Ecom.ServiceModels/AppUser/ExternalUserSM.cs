using Siffrum.Ecom.ServiceModels.Enums;
using Siffrum.Ecom.ServiceModels.Foundation.Base;

namespace Siffrum.Ecom.ServiceModels.AppUser
{
    public class ExternalUserSM : SiffrumServiceModelBase<long>
    {
        public string IdToken { get; set; }

        public long UserId { get; set; }

        public ExternalUserTypeSM ExternalUserType { get; set; }

        public RoleTypeSM RoleType { get; set; } 


    }
}
